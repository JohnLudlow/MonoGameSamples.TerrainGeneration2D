// Node script to validate agent changes in a PR context.
// Designed for dry-run mode: logs warnings instead of failing the workflow.

let github = null;
try {
  github = require('@actions/github');
} catch (e) {
  console.warn('Optional dependency @actions/github not available; running in limited dry-run mode.');
}
const fs = require('fs');
const path = require('path');

async function run() {
  // Environment expects GITHUB_EVENT_PATH that points to the event payload JSON on runners.
  const eventPath = process.env.GITHUB_EVENT_PATH;
  if (!eventPath || !fs.existsSync(eventPath)){
    console.warn('GITHUB_EVENT_PATH not set or not present; running in local/dry context.');
    return;
  }
  const event = JSON.parse(fs.readFileSync(eventPath,'utf8'));
  const pr = event.pull_request;
  if (!pr) {
    console.warn('No pull_request payload found; nothing to validate.');
    return;
  }

  const token = process.env.GITHUB_TOKEN || process.env.INPUT_GITHUB_TOKEN;
  if (!token) {
    console.warn('No GITHUB_TOKEN provided; will run in read-only dry-run mode.');
  }

  const octokit = (token && github) ? new github.GitHub(token) : null;

  // Fetch changed files
  let files = [];
  if (octokit) {
    const owner = process.env.GITHUB_REPOSITORY.split('/')[0];
    const repo = process.env.GITHUB_REPOSITORY.split('/')[1];
    files = await octokit.paginate(octokit.pulls.listFiles, {owner, repo, pull_number: pr.number});
    files = files.map(f => f.filename);
  } else {
    console.warn('Cannot list changed files without token; skipping file checks.');
  }

  // Load agent manifests and scan for unsupported fields
  const manifestsDir = path.join(process.cwd(), '.github', 'agents');
  const manifests = {};
  const allowedFields = new Set(['description','name','tools','forbidden_paths','commit_allowed']);
  const unsupportedFieldIssues = [];
  if (fs.existsSync(manifestsDir)){
    for (const mf of fs.readdirSync(manifestsDir)){
      if (!mf.endsWith('.md')) continue;
      const p = path.join(manifestsDir, mf);
      const content = fs.readFileSync(p, 'utf8');
      const nameMatch = content.match(/name:\s*(.*)/);
      if (nameMatch) manifests[nameMatch[1].trim()] = content;

      // Parse front-matter (very simple YAML-ish parser for keys before first blank line)
      const lines = content.split(/\r?\n/);
      let inFront = false;
      for (const ln of lines){
        if (ln.trim() === '---'){
          inFront = !inFront;
          continue;
        }
        if (!inFront) break;
        const m = ln.match(/^([a-zA-Z0-9_]+):/);
        if (m){
          const key = m[1];
          if (!allowedFields.has(key)){
            const msg = `Unsupported agent manifest field in ${mf}: ${key}`;
            console.warn(msg);
            unsupportedFieldIssues.push(msg);
          }
        }
      }
    }
  }
  console.log('Loaded agent manifests:', Object.keys(manifests));

  // Validate changed files
  const issues = unsupportedFieldIssues.slice();
  for (const f of files){
    if (f.startsWith('.github/agents/')) continue;
    if (f.endsWith('.cs')){
      const msg = 'C# source files must not be changed by agents: ' + f;
      console.warn(msg);
      issues.push(msg);
      continue;
    }
    if (!f.startsWith('docs/') && !f.startsWith('.github/')){
      const msg = 'Agents may only modify files under docs/ or .github/: ' + f;
      console.warn(msg);
      issues.push(msg);
      continue;
    }
    let content='';
    try { content = fs.readFileSync(f,'utf8'); } catch(e){}
    if (content && /agent:\s*(FeaturePlanner|ImplementationDocumenter|FeatureImplementer)/.test(content)){
      // ok
    }
  }

  let report = [];
  report.push('# Agent PR Validation Report');
  report.push('\n');
  report.push('PR: ' + pr.html_url + ' (number ' + pr.number + ')');
  report.push('\n');
  report.push('Mode: dry-run');
  report.push('\n');
  if (issues.length === 0) {
    const msg = 'PR validation (dry-run) passed — no issues found for PR #' + pr.number;
    console.log(msg);
    report.push('\n');
    report.push('**Status: PASS**');
    report.push('\n');
    report.push('No issues were detected.');
  } else {
    const msg = 'PR validation (dry-run) completed with issues for PR #' + pr.number + ':';
    console.log(msg + '\n' + issues.join('\n'));
    report.push('\n');
    report.push('**Status: WARNINGS DETECTED**');
    report.push('\n');
    report.push('Issues:');
    report.push('\n');
    for (const it of issues) report.push('- ' + it);
  }

  // Write report to artifact file
  const outDir = './test-results';
  if (!fs.existsSync(outDir)) fs.mkdirSync(outDir, { recursive: true });
  const outPath = path.join(outDir, 'agent-pr-validation.md');
  fs.writeFileSync(outPath, report.join('\n'), 'utf8');
  console.log('Wrote validation report to', outPath);
}

run().catch(err => {
  console.error('Validator failed:', err);
  process.exit(2);
});
