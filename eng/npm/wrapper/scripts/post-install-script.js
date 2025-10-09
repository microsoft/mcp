const os = require('os');

const platform = os.platform();
const arch = os.arch();

const baseName = process.env.PACKAGE_BASE_NAME || '@azure/mcp';
const requiredPackage = `${baseName}-${platform}-${arch}`;

try {
  require.resolve(requiredPackage);
} catch (err) {
  console.error(`Missing required package: '${requiredPackage}'. Follow the troubleshooting steps - https://aka.ms/azmcp/troubleshooting#platform-package-installation-issues`);
  process.exit(1);
}
