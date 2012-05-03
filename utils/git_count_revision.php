<?php
echo count(explode("\n", `git rev-list --all`));