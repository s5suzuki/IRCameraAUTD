# IRCameraAUTD

Optris Pi 450 ir camera viewer with AUTD3 controller

# Usage

## Set up AUTD

1. Expand the menu on the left and set the AUTD Geometry in the `Geometry` tab
2. In the `Link` tab, select link interface and click `Open` button
3. In the `Gain` tab, select `Gain` to display, then click right botton `+` button
    * Or, you can also use `PointSequence` in `Sequence` tab 
4. In the `Modulation` tab, select `Modulation` to append, then click right botton `+` button

* **The above settings will be saved, so you do not need to set again in the next time**

## Set up Pi 450

* Go to `Home` tab, and click `Connect` button

* Then, click `Start` button, AUTD3 will produce `Gain` you specified 
    * AUTD3 will stop if you click `Stop` button

## Memo 

* IRImagerDirect SDK version 8.7.0 does not work, so this program use 8.6.0

# Author

Shun Suzuki, 2021
