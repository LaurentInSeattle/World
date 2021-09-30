namespace Lyt.World.Model
{
    using System;

    // Model Variables: Creates the Agricultural Sector - Equations #84 to 128
    public sealed partial class Model
    {
        private Level arableLand;
        private Level potentiallyArableLand;
        private Level urbanIndustrialLand;
        private Level landFertility; 

        private Rate landDevelopmentRate;
        private Rate landErosionRate;
        private Rate landRemovalForUrbanIndustrialUse;
        private Rate landFertilityDegradation;
        private Rate landFertilityRegeneration; 

        private Auxiliary food; 
        private Auxiliary landFractionCultivated;
        private Auxiliary foodPerCapita;
        private Auxiliary fractionOfIndustrialOutputAllocatedToAgriculture;
        private Auxiliary agriculturalInputsPerHectare;
        private Auxiliary indicatedFoodPerCapita;
        private Auxiliary totalAgriculturalInvestment;
        private Auxiliary averageLifetimeOfAgriculturalInputs;
        private Auxiliary currentAgriculturalInputs;
        private Auxiliary landYield;
        private Auxiliary landYieldMultiplierFromAirPollution;
        private Auxiliary landYieldFactor;
        private Auxiliary marginalProductivityOfLandDevelopment;
        private Auxiliary marginalProductivityOfAgriculturalInputs;
        private Auxiliary averageLifeOfLand;
        private Auxiliary landLifeMultiplierFromYield;
        private Auxiliary urbanIndustrialLandRequired;
        private Auxiliary foodRatio; 

        private Table indicatedFoodPerCapitaBefore;
        private Table indicatedFoodPerCapitaAfter;
        private Table fractionOfIndustrialOutputAllocatedToAgricultureBefore;
        private Table fractionOfIndustrialOutputAllocatedToAgricultureAfter;
        private Table developmentCostPerHectare;
        private Table landYieldMultiplierFromCapital;
        private Table landYieldMultiplierFromAirPollutionBefore;
        private Table landYieldMultiplierFromAirPollutionAfter;
        private Table fractionOfInputsAllocatedToLandDevelopment;
        private Table marginalLandYieldMultiplierFromCapital;
        private Table landLifeMultiplierFromYieldBefore;
        private Table landLifeMultiplierFromYieldAfter;
        private Table urbanIndustrialLandPerCapita;
        private Table landFertilityDegradationRate;
        private Table landFertilityRegenerationTime;
        private Table fractionOfInputsAllocatedToLandMaintenance; 

        private Smooth agriculturalInputs;
        private Smooth perceivedFoodRatio; 

        private void CreateAgriculturalSector()
        {
            this.sector = "Agriculture";

            // Loop 1: Food from Investment in Land Development
            this.subSector = "Food from Investment in Land Development";


            const double landFractionCultivatedPotentiallyArableLandTotal = 3.2e9;  // hectares, used here 84 and in eqn 97
            this.landFractionCultivated = new Auxiliary(this, "landFractionCultivated", 84)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return arableLand.K / landFractionCultivatedPotentiallyArableLandTotal; }
            };

            this.arableLand = new Level(this, "arableLand", 85, "hectares", 0.9e9)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return
                        arableLand.J +
                        this.DeltaTime * (landDevelopmentRate.J - landErosionRate.J - landRemovalForUrbanIndustrialUse.J);
                }
            };

            this.potentiallyArableLand = new Level(this, "potentiallyArableLand", 86, "hectares", 2.3e9)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return potentiallyArableLand.J + this.DeltaTime * (-landDevelopmentRate.J);
                }
            };

            const double foodLandFractionHarvestedK = 0.7;   // dimensionless
            const double foodProcessingLossK = 0.1;          // dimensionless
            this.food = new Auxiliary(this, "food", 87, "kilograms per year")
            {
                UpdateFunction = delegate ()
                {
                    return landYield.K * arableLand.K * foodLandFractionHarvestedK * (1 - foodProcessingLossK);
                }
            };

            this.foodPerCapita = new Auxiliary(this, "foodPerCapita", 88, "kilograms per person-year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return food.K / population.K; }
            };

            this.indicatedFoodPerCapita = new Auxiliary(this, "indicatedFoodPerCapita", 89, "kilograms per person-year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                // Bug ??? Both 'After' and 'Before' tables below are identical 
                UpdateFunction = delegate ()
                {
                    return Clip(indicatedFoodPerCapitaAfter.K, indicatedFoodPerCapitaBefore.K, this.Time, Model.PolicyYear);
                }
            };

            this.indicatedFoodPerCapitaBefore =
                new Table(this, "indicatedFoodPerCapitaBefore", 90, "kilograms per person-year",
                new double[] { 230, 480, 690, 850, 970, 1070, 1150, 1210, 1250 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.indicatedFoodPerCapitaAfter =
                new Table(this, "indicatedFoodPerCapitaAfter", 91, "kilograms per person-year",
                new double[] { 230, 480, 690, 850, 970, 1070, 1150, 1210, 1250 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.totalAgriculturalInvestment = new Auxiliary(this, "totalAgriculturalInvestment", 92, "dollars per year")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate () { return industrialOutput.K * fractionOfIndustrialOutputAllocatedToAgriculture.K; }
            };


            this.fractionOfIndustrialOutputAllocatedToAgriculture =
                new Auxiliary(this, "fractionOfIndustrialOutputAllocatedToAgriculture", 93)
                {
                    CannotBeNegative = true,
                    // Bug ??? Both 'After' and 'Before' tables below are identical 
                    UpdateFunction = delegate ()
                    {
                        return Clip(
                            fractionOfIndustrialOutputAllocatedToAgricultureAfter.K,
                            fractionOfIndustrialOutputAllocatedToAgricultureBefore.K,
                            this.Time, Model.PolicyYear);
                    }
                };

            this.fractionOfIndustrialOutputAllocatedToAgricultureBefore =
                new Table(this, "fractionOfIndustrialOutputAllocatedToAgricultureBefore", 94, "dimensionless",
                new double[] { 0.4, 0.2, 0.1, 0.025, 0, 0 }, 0, 2.5, 0.5)
                {
                    UpdateFunction = delegate () { return foodPerCapita.K / indicatedFoodPerCapita.K; }
                };

            this.fractionOfIndustrialOutputAllocatedToAgricultureAfter =
                new Table(this, "fractionOfIndustrialOutputAllocatedToAgricultureAfter", 95, "dimensionless",
                new double[] { 0.4, 0.2, 0.1, 0.025, 0, 0 }, 0, 2.5, 0.5)
                {
                    UpdateFunction = delegate () { return foodPerCapita.K / indicatedFoodPerCapita.K; }
                };

            this.landDevelopmentRate = new Rate(this, "landDevelopmentRate", 96, "hectares per year")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return
                        totalAgriculturalInvestment.K * fractionOfInputsAllocatedToLandDevelopment.K / developmentCostPerHectare.K;
                }
            };


            this.developmentCostPerHectare =
                new Table(this, "developmentCostPerHectare", 97, "dollars per hectare",
                new double[] { 100000, 7400, 5200, 3500, 2400, 1500, 750, 300, 150, 75, 50 }, 0, 1.0, 0.1)
                {
                    CannotBeNegative = true,
                    CannotBeZero = true,
                    UpdateFunction = delegate ()
                    {
                        return potentiallyArableLand.K / landFractionCultivatedPotentiallyArableLandTotal;
                    }
                };


            // Loop 2: Food from Investment in Agricultural Inputs
            this.subSector = "Food from Investment in Agricultural Inputs";

            this.currentAgriculturalInputs = new Auxiliary(this, "currentAgriculturalInputs", 98, "dollars per year")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return totalAgriculturalInvestment.K / (1 - fractionOfInputsAllocatedToLandDevelopment.K);
                }
            };

            const double averageLifetimeOfAgriculturalInputsK = 2.0; // years, eqn 99 (in lieu of 100)
            const double agriculturalInputsInitVal = 5.0e9;
            this.agriculturalInputs =
                new Smooth(
                    this, "agriculturalInputs", 99, "dollars per year",
                    averageLifetimeOfAgriculturalInputsK, "currentAgriculturalInputs", agriculturalInputsInitVal);

            // note: output of this equation goes unused
            const double averageLifetimeOfAgriculturalInputsBefore = 2.0;
            const double averageLifetimeOfAgriculturalInputsAfter = 2.0;
            this.averageLifetimeOfAgriculturalInputs = new Auxiliary(this, "averageLifetimeOfAgriculturalInputs", 100, "years")
            {
                // Bug ??? Both 'After' and 'Before' tables below are identical 
                UpdateFunction = delegate ()
                {
                    // Bug ??? Both 'After' and 'Before' values above are identical 
                    return Clip(
                        averageLifetimeOfAgriculturalInputsAfter, averageLifetimeOfAgriculturalInputsBefore,
                        this.Time, Model.PolicyYear);
                }
            };

            this.agriculturalInputsPerHectare = new Auxiliary(this, "agriculturalInputsPerHectare", 101, "dollars per hectare-year")
            {
                UpdateFunction = delegate ()
                {
                    return agriculturalInputs.K * (1.0 - fractionOfInputsAllocatedToLandMaintenance.K ) / arableLand.K;
                }
            };


            this.landYieldMultiplierFromCapital =
                new Table(this, "landYieldMultiplierFromCapital", 102, "dimensionless",
                    new double[]
                    { 1, 3, 3.8, 4.4, 4.9, 5.4, 5.7, 6, 6.3, 6.6, 6.9, 7.2, 7.4, 7.6, 7.8, 8, 8.2, 8.4, 8.6, 8.8, 9, 9.2, 9.4, 9.6, 9.8, 10 },
                    0, 1000, 40)
                {
                    UpdateFunction = delegate () { return agriculturalInputsPerHectare.K; }
                };

            this.landYield = new Auxiliary(this, "landYield", 103, "kilograms per hectare-year")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return landYieldFactor.K * landFertility.K * landYieldMultiplierFromCapital.K * landYieldMultiplierFromAirPollution.K;
                }
            };

            const double landYieldFactorBefore = 1.0;
            const double landYieldFactorAfter = 1.0;
            this.landYieldFactor = new Auxiliary(this, "landYieldFactor", 104)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    // Bug ??? Both 'After' and 'Before' const values above are identical 
                    return Clip( landYieldFactorAfter, landYieldFactorBefore, this.Time, Model.PolicyYear);
                }
            };

            this.landYieldMultiplierFromAirPollution = new Auxiliary(this, "landYieldMultiplierFromAirPollution", 105)
            {
                UpdateFunction = delegate ()
                {
                    // Bug ??? Both 'After' and 'Before' tables below are identical 
                    return Clip(
                        landYieldMultiplierFromAirPollutionAfter.K, landYieldMultiplierFromAirPollutionBefore.K,
                        this.Time, Model.PolicyYear);
                }
            };

            const double industrialOutputValueIn1970 = 7.9e11;   // for eqns 106 and 107

            this.landYieldMultiplierFromAirPollutionBefore =
                new Table(this, "landYieldMultiplierFromAirPollutionBefore", 106, "dimensionless",
                new double[] { 1, 1, 0.7, 0.4 }, 0, 30, 10)
                {
                    UpdateFunction = delegate () { return industrialOutput.K / industrialOutputValueIn1970; }
                };

            this.landYieldMultiplierFromAirPollutionAfter =
                new Table(this, "landYieldMultiplierFromAirPollutionAfter", 107, "dimensionless",
                new double[] { 1, 1, 0.7, 0.4 }, 0, 30, 10)
                {
                    UpdateFunction = delegate () { return industrialOutput.K / industrialOutputValueIn1970; }
                };


            // Loops 1 and 2: The Investment Allocation Decision
            this.subSector = "Agricultural Investment Allocation Decision";


            this.fractionOfInputsAllocatedToLandDevelopment =
                new Table(this, "fractionOfInputsAllocatedToLandDevelopment", 108, "dimensionless",
                new double[] { 0, 0.05, 0.15, 0.30, 0.50, 0.70, 0.85, 0.95, 1 }, 0, 2, 0.25)
                {
                    UpdateFunction = delegate ()
                    {
                        return marginalProductivityOfLandDevelopment.K / marginalProductivityOfAgriculturalInputs.K;
                    }
                };

            const double marginalProductivityOfLandDevelopmentSocialDiscount = 0.07;
            this.marginalProductivityOfLandDevelopment =
                new Auxiliary(this, "marginalProductivityOfLandDevelopment", 109, "kilograms per dollar")
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            landYield.K / (developmentCostPerHectare.K * marginalProductivityOfLandDevelopmentSocialDiscount);
                    }
                };

            this.marginalProductivityOfAgriculturalInputs =
                new Auxiliary(this, "marginalProductivityOfAgriculturalInputs", 110, "kilograms per dollar")
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            averageLifetimeOfAgriculturalInputsK * landYield.K *
                            (marginalLandYieldMultiplierFromCapital.K * landYieldMultiplierFromCapital.K);
                    }
                };

            this.marginalLandYieldMultiplierFromCapital =
                new Table(this, "marginalLandYieldMultiplierFromCapital", 111, "hectares per dollar",
                new double[]
                {
                    0.075, 0.03, 0.015, 0.011, 0.009, 0.008, 0.007, 0.006, 0.005, 0.005, 0.005, 0.005, 0.005, 0.005, 0.005, 0.005
                }, 0, 600, 40)
                {
                    UpdateFunction = delegate () { return agriculturalInputsPerHectare.K; }
                };

            // Loop 3: Land Erosion and Urban-Industrial Use
            this.subSector = "Land Erosion and Urban-Industrial Use";

            const double averageLifeOfLandNormal = 6000.0;     // years
            this.averageLifeOfLand = new Auxiliary(this, "averageLifeOfLand", 112, "years")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate () { return averageLifeOfLandNormal * landLifeMultiplierFromYield.K; }
            };

            this.landLifeMultiplierFromYield = new Auxiliary(this, "landLifeMultiplierFromYield", 113)
            {
                // Bug ??? Both 'After' and 'Before' tables below are identical 
                UpdateFunction = delegate ()
                {
                    return
                        Clip(landLifeMultiplierFromYieldAfter.K, landLifeMultiplierFromYieldBefore.K,
                        this.Time, Model.PolicyYear);
                }
            };

            const double inherentLandFertilityK = 600.0;   // kilograms per hectare-year, used in eqns 114, 115 and 124
            this.landLifeMultiplierFromYieldBefore =
                new Table(this, "landLifeMultiplierFromYieldBefore", 114, "dimensionless",
                new double[] { 1.2, 1, 0.63, 0.36, 0.16, 0.055, 0.04, 0.025, 0.015, 0.01 }, 0, 9, 1)
                {
                    UpdateFunction = delegate () { return landYield.K / inherentLandFertilityK; }
                };

            this.landLifeMultiplierFromYieldAfter =
                new Table(this, "landLifeMultiplierFromYieldAfter", 115, "dimensionless",
                new double[] { 1.2, 1, 0.63, 0.36, 0.16, 0.055, 0.04, 0.025, 0.015, 0.01 }, 0, 9, 1)
                {
                    UpdateFunction = delegate () { return landYield.K / inherentLandFertilityK; }
                };

            this.landErosionRate = new Rate(this, "landErosionRate", 116, "hectares per year")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return arableLand.K / averageLifeOfLand.K; }
            };


            // 2016-08-09: Neil S. Grant reported an error in the table of values
            // for urbanIndustrialLandPerCapita. The third element of the array
            // should be 0.015, not 0.15. Corrected.
            this.urbanIndustrialLandPerCapita =
                new Table(this, "urbanIndustrialLandPerCapita", 117, "hectares per person",
                new double[] { 0.005, 0.008, 0.015, 0.025, 0.04, 0.055, 0.07, 0.08, 0.09 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.urbanIndustrialLandRequired = new Auxiliary(this, "urbanIndustrialLandRequired", 118, "hectares")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return urbanIndustrialLandPerCapita.K * population.K; }
            };

            const double landRemovalForUrbanIndustrialUseDevelopmentTime = 10.0;   // years
            this.landRemovalForUrbanIndustrialUse = new Rate(this, "landRemovalForUrbanIndustrialUse", 119, "hectares per year")
            {
                UpdateFunction = delegate ()
                {
                    return Math.Max(
                        0,
                        (urbanIndustrialLandRequired.K - urbanIndustrialLand.K) / landRemovalForUrbanIndustrialUseDevelopmentTime);
                }
            };

            this.urbanIndustrialLand = new Level(this, "urbanIndustrialLand", 120, "hectares", 8.2e6)
            {
                UpdateFunction = delegate ()
                {
                    return urbanIndustrialLand.J + this.DeltaTime * landRemovalForUrbanIndustrialUse.J;
                }
            };

            // Loop 4 and 5 : Land fertility
            this.subSector = "Land Fertility";

            // Loop 4 : Land fertility degradation

            this.landFertility = new Level(this, "landFertility", 121, "kilograms per hectare-year", 600)
            {
                CannotBeNegative = true,
                UpdateFunction = delegate ()
                {
                    return landFertility.J + this.DeltaTime * (landFertilityRegeneration.J - landFertilityDegradation.J);
                }
            };

            this.landFertilityDegradationRate =
                new Table(this, "landFertilityDegradationRate", 122, "inverse years", new double[] { 0, 0.1, 0.3, 0.5 }, 0, 30, 10)
                {
                    UpdateFunction = delegate () { return indexOfPersistentPollution.K; }
                };

            this.landFertilityDegradation = new Rate(this, "landFertilityDegradation", 123, "kilograms per hectare-year-year")
            {
                CannotBeNegative = true,
                UpdateFunction = delegate () { return landFertility.K * landFertilityDegradationRate.K; }
            };

            // Loop 5: Land fertility regeneration


            this.landFertilityRegeneration = new Rate(this, "landFertilityRegeneration", 124, "kilograms per hectare-year-year")
            {
                UpdateFunction = delegate ()
                {
                    return (inherentLandFertilityK - landFertility.K) / landFertilityRegenerationTime.K;
                }
            };


            this.landFertilityRegenerationTime =
                new Table(this, "landFertilityRegenerationTime", 125, "years", new double[] { 20, 13, 8, 4, 2, 2 }, 0, 0.1, 0.02)
                {
                    UpdateFunction = delegate () { return fractionOfInputsAllocatedToLandMaintenance.K; }
                };

            // Loop 6: Land maintenance
            this.subSector = "Land Maintenance";

            this.fractionOfInputsAllocatedToLandMaintenance =
                new Table(this, "fractionOfInputsAllocatedToLandMaintenance", 126, "dimensionless",
                new double[] { 0, 0.04, 0.07, 0.09, 0.10 }, 0, 4, 1)
                {
                    UpdateFunction = delegate () { return perceivedFoodRatio.K; }
                };

            this.foodRatio = new Auxiliary(this, "foodRatio", 127)
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate () { return foodPerCapita.K / subsistenceFoodPerCapitaK; }
            };

            const double foodShortagePerceptionDelayK = 2.0;  // years, used in eqn 128
            const double perceivedFoodRatioInitVal = 1.0;
            this.perceivedFoodRatio =
                new Smooth(
                    this, "perceivedFoodRatio", 128, "dimensionless", foodShortagePerceptionDelayK, 
                    "foodRatio", perceivedFoodRatioInitVal);
        }
    }
}