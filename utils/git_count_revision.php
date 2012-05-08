<?php
echo count(explode("\n", trim(`git rev-list --all`))) + 0;