# Bamboo Monitor
A tool to launch builds in Bamboo consuming the task info from TTS, our custom Issue Tracker at Codice Software.

# Motivation
We'd like to spend as little time as possible on our release building process. It would be incredibly helpful
if our task branches could be automatically merged into main (master in git jargon) without human supervision.
To achieve that, we've set up a Bamboo CI server which monitors our Plastic SCM production server as a git server
(have a look at [the details](https://www.plasticscm.com/gitserver/)). 

The thing is, Bamboo doesn't allow conditions related to the push action of the automatic merge in the build configuration.
We don't want our branches to be integrated until they are marked as validated in our issue tracker system. We recommend
you to read our [branch per task guide](https://www.plasticscm.com/branch-per-task-guide/), in case you haven't already.

This means that we need some external program to check all the conditions to trigger our integrator build plans.
That plan is configured to include a merge to main and push back the new commit to the source server if the build
process ends with success. It also modifies the `status` attribute of the affected branch in Plastic SCM to acknowledge
the integration and updates the TTS with this information.

# How it works
The program retrieves all branches in the Plastic SCM server having their `status` attribute set to `RESOLVED` and
checks in TTS whether they have been already validated or not. Already integrated branches are excluded, too. After that,
retrieves the appropriate build plan for each branch and triggers a build. The application will exit once all required plans
have had a build triggered, so this is *not* a daemon. It's conceived to be ran as a cron job or as a scheduled task.

# Configuration
The Bamboo monitor requires a configuration file to store all customizable variables. It needs to be place in the same directory
as the executable `bamboomonitor.exe` and named as `bamboomonitor.conf`. The required variables are:
* `tts.server`: The base HTTP(S) URI of the TTS service
* `tts.user`: User name to authorize TTS requests
* `tts.password`: Password to authorize TTS requests
* `plastic.repo`: Full spec of the targeted Plastic SCM repository
* `plastic.branchPrefix`: Prefix to expect when monitoring branches
* `bamboo.server`: The base HTTP(S) URI of the Bamboo service
* `bamboo.plan`: The master plan key where the plan branches will be searched
* `bamboo.user`: User name to authorize Bamboo REST API requests
* `bamboo.password`: Password to authorize Bamboo REST API requests

Sample `bamboomonitor.conf`:
```
tts.server=https://tts.plasticscm.com
tts.user=user
tts.password=pass
plastic.repo=codice@production.plasticscm.com:7100
plastic.branchPrefix=scm
bamboo.server=https://bamboo.plasticscm.com
bamboo.plan=MY-PLN
bamboo.user=bamboo-user
bamboo.password=passwd
```

Once everything is ready, just run `bamboomonitor.exe` on your command prompt or let `cron` or a scheduled task handle it.