#! /bin/bash

tsc
rollup -c
rm src/*.js

date +"%T"
ls -lh ../Server/wwwroot