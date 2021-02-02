#!/usr/bin/env node

const { getActiveSync, getByPidSync } = require('..');
const args = process.argv.slice(2);

if (args.length) {
  console.log(getByPidSync(args[0]));
} else {
  console.log(getActiveSync());
}