@echo off
ECHO GIT COMMIT
git commit -am "update version[ci skip]"
git branch tmp
git checkout Medio74
git merge tmp
git branch -d tmp
git push