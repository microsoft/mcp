const fs = require('fs')
const path = require('path')
const os = require('os');

const platform = os.platform();
const arch = os.arch();

const pkgJsonPath = path.join(__dirname, '..', 'package.json');
let baseName = '@azure/mcp';
try {
  const pkg = JSON.parse(fs.readFileSync(pkgJsonPath, 'utf8'));
  if (pkg.name) {
    baseName = pkg.name;
  }
} catch (e) {
  // fallback to default
}

const requiredPackage = `${baseName}-${platform}-${arch}`;

try {
  require.resolve(requiredPackage);
} catch (err) {
  console.error(`Missing required package: '${requiredPackage}'. Follow the troubleshooting steps - https://aka.ms/azmcp/troubleshooting#platform-package-installation-issues`);
  process.exit(1);
}
