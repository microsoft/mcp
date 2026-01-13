const os = require('os');

const platform = os.platform();
const arch = os.arch();

let baseName = '';
try{
    const packageJson = require('../package.json');
    baseName = packageJson.name;
}
catch (err) {
  console.error('Unable to verify platform package installation. Error reading package.json.');
  process.exit(1);
}

const requiredPackage = `${baseName}-${platform}-${arch}`;
try {
  require.resolve(requiredPackage);
} catch (err) {
  console.error(`Missing required package: '${requiredPackage}'. Follow the troubleshooting steps - https://aka.ms/azmcp/troubleshooting#platform-package-installation-issues`);
  process.exit(1);
}
