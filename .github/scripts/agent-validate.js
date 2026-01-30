// Simple Node validator for agent PRs
const fs = require('fs');
const path = require('path');

function loadAgentManifests(){
  const dir = path.join(__dirname, '..', 'agents');
  const manifests = {};
  if (!fs.existsSync(dir)) return manifests;
  for (const f of fs.readdirSync(dir)){
    if (!f.endsWith('.md')) continue;
    const content = fs.readFileSync(path.join(dir,f),'utf8');
    const nameMatch = content.match(/name:\s*(.*)/);
    if (nameMatch){
      manifests[nameMatch[1].trim()] = content;
    }
  }
  return manifests;
}

function main(){
  const manifests = loadAgentManifests();
  console.log('Loaded agent manifests:', Object.keys(manifests));
}

if (require.main === module) main();

module.exports = { loadAgentManifests };
