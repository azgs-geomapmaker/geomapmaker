﻿using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Geomapmaker.Data
{
    public class Intervals
    {
        // Sourced from https://macrostrat.org/api/v1/defs/intervals?timescale=international%20intervals on 9/30/2021
        // I wrote a script to convert from JSON to this initialized list in the Scripts folder
        public static ObservableCollection<Interval> IntervalOptions => new ObservableCollection<Interval>() {
            new Interval { Name="Holocene", Early_Age=0.0117, Late_Age=0, Type="Epoch" },
            new Interval { Name="Late Pleistocene", Early_Age=0.126, Late_Age=0.0117, Type="Age" },
            new Interval { Name="Middle Pleistocene", Early_Age=0.781, Late_Age=0.126, Type="Age" },
            new Interval { Name="Calabrian", Early_Age=1.806, Late_Age=0.781, Type="Age" },
            new Interval { Name="Quaternary", Early_Age=2.588, Late_Age=0, Type="Period" },
            new Interval { Name="Pleistocene", Early_Age=2.588, Late_Age=0.0117, Type="Epoch" },
            new Interval { Name="Gelasian", Early_Age=2.588, Late_Age=1.806, Type="Age" },
            new Interval { Name="Piacenzian", Early_Age=3.6, Late_Age=2.588, Type="Age" },
            new Interval { Name="Pliocene", Early_Age=5.333, Late_Age=2.588, Type="Epoch" },
            new Interval { Name="Zanclean", Early_Age=5.333, Late_Age=3.6, Type="Age" },
            new Interval { Name="Messinian", Early_Age=7.246, Late_Age=5.333, Type="Age" },
            new Interval { Name="Tortonian", Early_Age=11.62, Late_Age=7.246, Type="Age" },
            new Interval { Name="Serravallian", Early_Age=13.82, Late_Age=11.62, Type="Age" },
            new Interval { Name="Langhian", Early_Age=15.97, Late_Age=13.82, Type="Age" },
            new Interval { Name="Burdigalian", Early_Age=20.44, Late_Age=15.97, Type="Age" },
            new Interval { Name="Miocene", Early_Age=23.03, Late_Age=5.333, Type="Epoch" },
            new Interval { Name="Neogene", Early_Age=23.03, Late_Age=2.588, Type="Period" },
            new Interval { Name="Aquitanian", Early_Age=23.03, Late_Age=20.44, Type="Age" },
            new Interval { Name="Chattian", Early_Age=28.1, Late_Age=23.03, Type="Age" },
            new Interval { Name="Oligocene", Early_Age=33.9, Late_Age=23.03, Type="Epoch" },
            new Interval { Name="Rupelian", Early_Age=33.9, Late_Age=28.1, Type="Age" },
            new Interval { Name="Priabonian", Early_Age=37.8, Late_Age=33.9, Type="Age" },
            new Interval { Name="Bartonian", Early_Age=41.3, Late_Age=37.8, Type="Age" },
            new Interval { Name="Lutetian", Early_Age=47.8, Late_Age=41.3, Type="Age" },
            new Interval { Name="Eocene", Early_Age=56, Late_Age=33.9, Type="Epoch" },
            new Interval { Name="Ypresian", Early_Age=56, Late_Age=47.8, Type="Age" },
            new Interval { Name="Thanetian", Early_Age=59.2, Late_Age=56, Type="Age" },
            new Interval { Name="Selandian", Early_Age=61.6, Late_Age=59.2, Type="Age" },
            new Interval { Name="Danian", Early_Age=66, Late_Age=61.6, Type="Age" },
            new Interval { Name="Cenozoic", Early_Age=66, Late_Age=0, Type="Era" },
            new Interval { Name="Paleogene", Early_Age=66, Late_Age=23.03, Type="Period" },
            new Interval { Name="Paleocene", Early_Age=66, Late_Age=56, Type="Epoch" },
            new Interval { Name="Maastrichtian", Early_Age=72.1, Late_Age=66, Type="Age" },
            new Interval { Name="Campanian", Early_Age=83.6, Late_Age=72.1, Type="Age" },
            new Interval { Name="Santonian", Early_Age=86.3, Late_Age=83.6, Type="Age" },
            new Interval { Name="Coniacian", Early_Age=89.8, Late_Age=86.3, Type="Age" },
            new Interval { Name="Turonian", Early_Age=93.9, Late_Age=89.8, Type="Age" },
            new Interval { Name="Late Cretaceous", Early_Age=100.5, Late_Age=66, Type="Epoch" },
            new Interval { Name="Cenomanian", Early_Age=100.5, Late_Age=93.9, Type="Age" },
            new Interval { Name="Albian", Early_Age=113, Late_Age=100.5, Type="Age" },
            new Interval { Name="Aptian", Early_Age=125, Late_Age=113, Type="Age" },
            new Interval { Name="Barremian", Early_Age=129.4, Late_Age=125, Type="Age" },
            new Interval { Name="Hauterivian", Early_Age=132.9, Late_Age=129.4, Type="Age" },
            new Interval { Name="Valanginian", Early_Age=139.8, Late_Age=132.9, Type="Age" },
            new Interval { Name="Berriasian", Early_Age=145, Late_Age=139.8, Type="Age" },
            new Interval { Name="Cretaceous", Early_Age=145, Late_Age=66, Type="Period" },
            new Interval { Name="Early Cretaceous", Early_Age=145, Late_Age=100.5, Type="Epoch" },
            new Interval { Name="Tithonian", Early_Age=152.1, Late_Age=145, Type="Age" },
            new Interval { Name="Kimmeridgian", Early_Age=157.3, Late_Age=152.1, Type="Age" },
            new Interval { Name="Late Jurassic", Early_Age=163.5, Late_Age=145, Type="Epoch" },
            new Interval { Name="Oxfordian", Early_Age=163.5, Late_Age=157.3, Type="Age" },
            new Interval { Name="Callovian", Early_Age=166.1, Late_Age=163.5, Type="Age" },
            new Interval { Name="Bathonian", Early_Age=168.3, Late_Age=166.1, Type="Age" },
            new Interval { Name="Bajocian", Early_Age=170.3, Late_Age=168.3, Type="Age" },
            new Interval { Name="Middle Jurassic", Early_Age=174.1, Late_Age=163.5, Type="Epoch" },
            new Interval { Name="Aalenian", Early_Age=174.1, Late_Age=170.3, Type="Age" },
            new Interval { Name="Toarcian", Early_Age=182.7, Late_Age=174.1, Type="Age" },
            new Interval { Name="Pliensbachian", Early_Age=190.8, Late_Age=182.7, Type="Age" },
            new Interval { Name="Sinemurian", Early_Age=199.3, Late_Age=190.8, Type="Age" },
            new Interval { Name="Jurassic", Early_Age=201.3, Late_Age=145, Type="Period" },
            new Interval { Name="Hettangian", Early_Age=201.3, Late_Age=199.3, Type="Age" },
            new Interval { Name="Early Jurassic", Early_Age=201.3, Late_Age=174.1, Type="Epoch" },
            new Interval { Name="Rhaetian", Early_Age=208.5, Late_Age=201.3, Type="Age" },
            new Interval { Name="Norian", Early_Age=227, Late_Age=208.5, Type="Age" },
            new Interval { Name="Late Triassic", Early_Age=237, Late_Age=201.3, Type="Epoch" },
            new Interval { Name="Carnian", Early_Age=237, Late_Age=227, Type="Age" },
            new Interval { Name="Ladinian", Early_Age=242, Late_Age=237, Type="Age" },
            new Interval { Name="Middle Triassic", Early_Age=247.2, Late_Age=237, Type="Epoch" },
            new Interval { Name="Anisian", Early_Age=247.2, Late_Age=242, Type="Age" },
            new Interval { Name="Olenekian", Early_Age=251.2, Late_Age=247.2, Type="Age" },
            new Interval { Name="Triassic", Early_Age=251.902, Late_Age=201.3, Type="Period" },
            new Interval { Name="Early Triassic", Early_Age=251.902, Late_Age=247.2, Type="Epoch" },
            new Interval { Name="Induan", Early_Age=251.902, Late_Age=251.2, Type="Age" },
            new Interval { Name="Mesozoic", Early_Age=251.902, Late_Age=66, Type="Era" },
            new Interval { Name="Changhsingian", Early_Age=254.14, Late_Age=251.902, Type="Age" },
            new Interval { Name="Wuchiapingian", Early_Age=259.1, Late_Age=254.14, Type="Age" },
            new Interval { Name="Lopingian", Early_Age=259.1, Late_Age=251.902, Type="Epoch" },
            new Interval { Name="Capitanian", Early_Age=265.1, Late_Age=259.1, Type="Age" },
            new Interval { Name="Wordian", Early_Age=268.8, Late_Age=265.1, Type="Age" },
            new Interval { Name="Guadalupian", Early_Age=272.95, Late_Age=259.1, Type="Epoch" },
            new Interval { Name="Roadian", Early_Age=272.95, Late_Age=268.8, Type="Age" },
            new Interval { Name="Kungurian", Early_Age=283.5, Late_Age=272.95, Type="Age" },
            new Interval { Name="Artinskian", Early_Age=290.1, Late_Age=283.5, Type="Age" },
            new Interval { Name="Sakmarian", Early_Age=295, Late_Age=290.1, Type="Age" },
            new Interval { Name="Permian", Early_Age=298.9, Late_Age=251.902, Type="Period" },
            new Interval { Name="Cisuralian", Early_Age=298.9, Late_Age=272.95, Type="Epoch" },
            new Interval { Name="Asselian", Early_Age=298.9, Late_Age=295, Type="Age" },
            new Interval { Name="Gzhelian", Early_Age=303.7, Late_Age=298.9, Type="Age" },
            new Interval { Name="Kasimovian", Early_Age=307, Late_Age=303.7, Type="Age" },
            new Interval { Name="Moscovian", Early_Age=315.2, Late_Age=307, Type="Age" },
            new Interval { Name="Bashkirian", Early_Age=323.2, Late_Age=315.2, Type="Age" },
            new Interval { Name="Pennsylvanian", Early_Age=323.2, Late_Age=298.9, Type="Epoch" },
            new Interval { Name="Serpukhovian", Early_Age=330.9, Late_Age=323.2, Type="Age" },
            new Interval { Name="Visean", Early_Age=346.7, Late_Age=330.9, Type="Age" },
            new Interval { Name="Carboniferous", Early_Age=358.9, Late_Age=298.9, Type="Period" },
            new Interval { Name="Mississippian", Early_Age=358.9, Late_Age=323.2, Type="Epoch" },
            new Interval { Name="Tournaisian", Early_Age=358.9, Late_Age=346.7, Type="Age" },
            new Interval { Name="Famennian", Early_Age=372.2, Late_Age=358.9, Type="Age" },
            new Interval { Name="Frasnian", Early_Age=382.7, Late_Age=372.2, Type="Age" },
            new Interval { Name="Late Devonian", Early_Age=382.7, Late_Age=358.9, Type="Epoch" },
            new Interval { Name="Givetian", Early_Age=387.7, Late_Age=382.7, Type="Age" },
            new Interval { Name="Middle Devonian", Early_Age=393.3, Late_Age=382.7, Type="Epoch" },
            new Interval { Name="Eifelian", Early_Age=393.3, Late_Age=387.7, Type="Age" },
            new Interval { Name="Emsian", Early_Age=407.6, Late_Age=393.3, Type="Age" },
            new Interval { Name="Pragian", Early_Age=410.8, Late_Age=407.6, Type="Age" },
            new Interval { Name="Early Devonian", Early_Age=419.2, Late_Age=393.3, Type="Epoch" },
            new Interval { Name="Lochkovian", Early_Age=419.2, Late_Age=410.8, Type="Age" },
            new Interval { Name="Devonian", Early_Age=419.2, Late_Age=358.9, Type="Period" },
            new Interval { Name="Pridoli", Early_Age=423, Late_Age=419.2, Type="Epoch" },
            new Interval { Name="Ludfordian", Early_Age=425.6, Late_Age=423, Type="Age" },
            new Interval { Name="Gorstian", Early_Age=427.4, Late_Age=425.6, Type="Age" },
            new Interval { Name="Ludlow", Early_Age=427.4, Late_Age=423, Type="Epoch" },
            new Interval { Name="Homerian", Early_Age=430.5, Late_Age=427.4, Type="Age" },
            new Interval { Name="Sheinwoodian", Early_Age=433.4, Late_Age=430.5, Type="Age" },
            new Interval { Name="Wenlock", Early_Age=433.4, Late_Age=427.4, Type="Epoch" },
            new Interval { Name="Telychian", Early_Age=438.5, Late_Age=433.4, Type="Age" },
            new Interval { Name="Aeronian", Early_Age=440.8, Late_Age=438.5, Type="Age" },
            new Interval { Name="Silurian", Early_Age=443.8, Late_Age=419.2, Type="Period" },
            new Interval { Name="Llandovery", Early_Age=443.8, Late_Age=433.4, Type="Epoch" },
            new Interval { Name="Rhuddanian", Early_Age=443.8, Late_Age=440.8, Type="Age" },
            new Interval { Name="Hirnantian", Early_Age=445.2, Late_Age=443.8, Type="Age" },
            new Interval { Name="Katian", Early_Age=453, Late_Age=445.2, Type="Age" },
            new Interval { Name="Late Ordovician", Early_Age=458.4, Late_Age=443.8, Type="Epoch" },
            new Interval { Name="Sandbian", Early_Age=458.4, Late_Age=453, Type="Age" },
            new Interval { Name="Darriwilian", Early_Age=467.3, Late_Age=458.4, Type="Age" },
            new Interval { Name="Middle Ordovician", Early_Age=470, Late_Age=458.4, Type="Epoch" },
            new Interval { Name="Dapingian", Early_Age=470, Late_Age=467.3, Type="Age" },
            new Interval { Name="Floian", Early_Age=477.7, Late_Age=470, Type="Age" },
            new Interval { Name="Tremadocian", Early_Age=485.4, Late_Age=477.7, Type="Age" },
            new Interval { Name="Ordovician", Early_Age=485.4, Late_Age=443.8, Type="Period" },
            new Interval { Name="Early Ordovician", Early_Age=485.4, Late_Age=470, Type="Epoch" },
            new Interval { Name="Stage 10", Early_Age=489.5, Late_Age=485.4, Type="Age" },
            new Interval { Name="Jiangshanian", Early_Age=494, Late_Age=489.5, Type="Age" },
            new Interval { Name="Furongian", Early_Age=497, Late_Age=485.4, Type="Epoch" },
            new Interval { Name="Paibian", Early_Age=497, Late_Age=494, Type="Age" },
            new Interval { Name="Guzhangian", Early_Age=500.5, Late_Age=497, Type="Age" },
            new Interval { Name="Drumian", Early_Age=504.5, Late_Age=500.5, Type="Age" },
            new Interval { Name="Miaolingian", Early_Age=509, Late_Age=497, Type="Epoch" },
            new Interval { Name="Wuliuan", Early_Age=509, Late_Age=504.5, Type="Age" },
            new Interval { Name="Stage 4", Early_Age=514, Late_Age=509, Type="Age" },
            new Interval { Name="Series 2", Early_Age=521, Late_Age=509, Type="Epoch" },
            new Interval { Name="Stage 3", Early_Age=521, Late_Age=514, Type="Age" },
            new Interval { Name="Stage 2", Early_Age=529, Late_Age=521, Type="Age" },
            new Interval { Name="Terreneuvian", Early_Age=541, Late_Age=521, Type="Epoch" },
            new Interval { Name="Fortunian", Early_Age=541, Late_Age=529, Type="Age" },
            new Interval { Name="Paleozoic", Early_Age=541, Late_Age=251.902, Type="Era" },
            new Interval { Name="Cambrian", Early_Age=541, Late_Age=485.4, Type="Period" },
            new Interval { Name="Ediacaran", Early_Age=635, Late_Age=541, Type="Period" },
            new Interval { Name="Cryogenian", Early_Age=720, Late_Age=635, Type="Period" },
            new Interval { Name="Neoproterozoic", Early_Age=1000, Late_Age=541, Type="Era" },
            new Interval { Name="Tonian", Early_Age=1000, Late_Age=720, Type="Period" },
            new Interval { Name="Stenian", Early_Age=1200, Late_Age=1000, Type="Period" },
            new Interval { Name="Ectasian", Early_Age=1400, Late_Age=1200, Type="Period" },
            new Interval { Name="Mesoproterozoic", Early_Age=1600, Late_Age=1000, Type="Era" },
            new Interval { Name="Calymmian", Early_Age=1600, Late_Age=1400, Type="Period" },
            new Interval { Name="Statherian", Early_Age=1800, Late_Age=1600, Type="Period" },
            new Interval { Name="Orosirian", Early_Age=2050, Late_Age=1800, Type="Period" },
            new Interval { Name="Rhyacian", Early_Age=2300, Late_Age=2050, Type="Period" },
            new Interval { Name="Paleoproterozoic", Early_Age=2500, Late_Age=1600, Type="Era" },
            new Interval { Name="Siderian", Early_Age=2500, Late_Age=2300, Type="Period" },
            new Interval { Name="Neoarchean", Early_Age=2800, Late_Age=2500, Type="Era" },
            new Interval { Name="Mesoarchean", Early_Age=3200, Late_Age=2800, Type="Era" },
            new Interval { Name="Paleoarchean", Early_Age=3600, Late_Age=3200, Type="Era" },
            new Interval { Name="Eoarchean", Early_Age=4000, Late_Age=3600, Type="Era" },
        };
    }
}
