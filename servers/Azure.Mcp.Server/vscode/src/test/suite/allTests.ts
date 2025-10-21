// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as path from 'path';
import * as Mocha from 'mocha';
import * as glob from 'glob';

export function run(): Promise<void> {
    const opts: Mocha.MochaOptions = {
        ui: 'tdd',
        color: true,
        timeout: process.env.TEST_TIMEOUT ?? "10s"
    };
    
    const mocha = new Mocha(opts);

    const testsRoot = path.resolve(__dirname, '..');

    return new Promise((c, e) => {
        glob('suite/**/**.test.js', { cwd: testsRoot }, (err, files) => {
            if (err) {
                return e(err);
            }

            files.forEach(f => mocha.addFile(path.resolve(testsRoot, f)));

            try {
                mocha.run(failures => {
                    if (failures > 0) {
                        e(new Error(`${failures} tests failed.`));
                    } else {
                        c();
                    }
                });
            } catch (err) {
                console.error(err);
                e(err);
            }
        });
    });
}
