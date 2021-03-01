<p align="center">
  <img src="https://github.com/joeltroughton/PV_stability_plotter/raw/master/Screenshot.png" width="600" title="μSMU">
</p>

# PV_stability_plotter
KAUST Solar Center's PV stability lab measures solar cells over extended periods of time and logs their vital statistics (VOC, JSC, Fill factor, Power conversion efficiency). These measurements are performed every few minutes and can last for thousands of hours. This application allows users to quickly skim through different devices to get a graphic overview of their stability

The application scans a folder for .txt files containinng stability data in the below format, where the elapsed time in hours is encoded after the COM5_ string.

```
File Name   Jsc(mA / cm2)    Voc(V) Pmax(mW / cm2)   Fill Factor Rect.Rato Rseries(Ohms)  Rshunt(Ohms)
COM5_0.022_0.dat	-1.27761E+1	1.13200E+0	-1.05220E+1	7.27531E-1
COM5_0.191_0.dat	-1.22039E+1	1.14082E+0	-9.86092E+0	7.08274E-1
COM5_0.359_0.dat	-1.18372E+1	1.14726E+0	-9.53171E+0	7.01877E-1
COM5_0.528_0.dat	-1.14961E+1	1.15380E+0	-9.22606E+0	6.95564E-1
COM5_0.696_0.dat	-1.11551E+1	1.15993E+0	-8.94033E+0	6.90952E-1
```

Data is displayed using the amazing [ScottPlot](https://github.com/ScottPlot/ScottPlot) library. Data can also be exported into an easier-to-graph format (Junk data discarded, JSC and Pmax values in absolute terms, time formatted)
