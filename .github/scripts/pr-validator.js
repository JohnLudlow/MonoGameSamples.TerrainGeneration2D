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

  // Load agent manifests
  const manifestsDir = path.join(process.cwd(), '.github', 'agents');
  const manifests = {};
  if (fs.existsSync(manifestsDir)){
    for (const mf of fs.readdirSync(manifestsDir)){
      if (!mf.endsWith('.md')) continue;
      const content = fs.readFileSync(path.join(manifestsDir, mf), 'utf8');
      const nameMatch = content.match(/name:\s*(.*)/);
      if (nameMatch) manifests[nameMatch[1].trim()] = content;
    }
  }
  console.log('Loaded agent manifests:', Object.keys(manifests));

  // Validate changed files
  for (const f of files){
    if (f.startsWith('.github/agents/')) continue;
    if (f.endsWith('.cs')){
      console.warn('C# source files must not be changed by agents: ' + f);
      continue;
    }
    if (!f.startsWith('docs/') && !f.startsWith('.github/')){
      console.warn('Agents may only modify files under docs/ or .github/: ' + f);
      continue;
    }
    let content='';
    try { content = fs.readFileSync(f,'utf8'); } catch(e){}
    if (content && /agent:\s*(FeaturePlanner|ImplementationDocumenter|FeatureImplementer)/.test(content)){
      // ok
    }
  }

  console.log('PR validation (dry-run) completed for PR #' + pr.number);
}

run().catch(err => {
  console.error('Validator failed:', err);
  process.exit(2);
});
