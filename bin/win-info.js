#!/usr/bin/env node

const { getActiveSync, getByPidSync } = require('..');
const args = process.argv.slice(2);

console.log(
  JSON.stringify(
    args.length ? getByPidSync(args[0]) : getActiveSync(),
    null, 2
  )
);