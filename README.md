# Static Balancing Tool for Rotor
StaticBalancing is a windows application for rotor balancing.

[![.Build Status](https://github.com/Dai0526/StaticBalancing/actions/workflows/StaticBalancingCI.yml/badge.svg)](https://github.com/Dai0526/StaticBalancing/actions/workflows/StaticBalancingCI.yml)

![Plot](https://github.com/Dai0526/StaticBalancing/blob/main/reference/Images/GUI.png)
---------------------------------------

## Overview

#### Customize the system configuration file for CT models
```xml
    <System>
      <Model>CT123</Model>
      <HomeTickOffset>3.0</HomeTickOffset>
      <MaxImbalance>100</MaxImbalance>
      <MaxSpeed>60</MaxSpeed>
      <BalancePositionList>
            <position id = "5Clock" radius = "230.0" angle = "165" maxStackHeight = "100.0" direction = "Radial-"/>
            <position id = "7Clock" radius = "530.0" angle = "195" maxStackHeight = "100.0" direction = "Radial-"/>
      </BalancePositionList>
      <CounterTypeList>
            <counter pn = "pn1" mass = "1.7" thickness = "6"/>          
            <counter pn = "pn2" mass = "0.7" thickness = "3"/>
            <counter pn = "pn3" mass = "0.3" thickness = "1"/>
            <counter pn = "pn4" mass = "10.2" thickness = "1"/>
      </CounterTypeList>
    </System> 
```

---------------------------------------
#### Export and Import History Data for further Analysis
The dumped file will be in .csv format with calibration result and imbalance informations, such that it could utlize the Excel/GoogleSheets to do further analysis.
The entry of dumped file has 3 parts:
1. Header
2. MetaInfo
3. Data

The first 3 row will be header info: 
| Key               | Value          |
|-------------------|----------------|
| Model             | CT123          |
| Serial            | ABC123456      |
| MaxImbalance      | 300            |
| HeaderSize        | 11             |

The 4th row is meta info, which describe the data format(column number of data). 
| Key               | Value          |
|-------------------|----------------|
| HeaderSize        | 11             |

Balancing Data is append after meta info.

| Timestamp               | Imbalance             | Angle    | Speed    | ... | Weight_7Clock      |
|-------------------------|-----------------------|----------|----------|-----|--------------------|
| 3/23/2021  10:21:15 AM  | 2056.13213784112      | 175.9712 | 10.9337  | ... | -3.40000009536743  |
| 3/23/2021  10:21:47 AM  | -78.391924309984      | -78.3919 | 14.15991 | ... | -1.39999997615814  | 


## Input Data Format
Current Release support 2 input data files:
* .csv
* .txt

Please refer to the sample data [Data Sample](https://github.com/Dai0526/StaticBalancing/tree/main/reference/Data)


## Reference

#### Links
[Taskboard](https://trello.com/b/YZb9DfgL/static-balancing-tool) \
[Design Doc](https://drive.google.com/drive/folders/1EheOILpD6LkiA7-EsfP2dNUdGCXV8nhQ?usp=sharing)
