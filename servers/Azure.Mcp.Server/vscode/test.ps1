if ($IsLinux) {
    xvfb-run -a npm run unit-test
} else {
    npm run unit-test
}