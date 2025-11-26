// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as path from 'path';
import * as Mocha from 'mocha';
import { glob } from 'glob';

export function run(): Promise<void> {
    const opts: Mocha.MochaOptions = {
        ui: 'tdd',
        color: true,
        timeout: process.env.TEST_TIMEOUT ?? "10s",
        slow: 200
    };

    const mocha = new Mocha(opts);
    const testsRoot = path.resolve(__dirname, '..');

    return new Promise((c, e) => {
        // Find both legacy and new-style unit tests
        Promise.all([
            glob('suite/unit/**/**.test.js', { cwd: testsRoot }),
            glob('*.unit.test.js', { cwd: testsRoot })
        ]).then(([suiteFiles, unitFiles]: [string[], string[]]) => {
            const files = [...suiteFiles, ...unitFiles];
            files.forEach((f: string) => mocha.addFile(path.resolve(testsRoot, f)));
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
        }).catch(e);
    });
}
