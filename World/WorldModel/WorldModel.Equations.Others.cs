namespace Lyt.World.Model
{
    using Lyt.World.Engine;

    // Model Variables: Creates Other Sectors - Equations #129 to #149
    public sealed partial class WorldModel
    {
        private Level nonrenewableResources;
        private Level persistentPollution;

        private Rate nonrenewableResourceUsageRate;
        private Rate persistentPollutionGenerationRate;
        private Rate persistenPollutionAssimilationRate;

        private Delay persistenPollutionAppearanceRate;

        private Auxiliary nonrenewableResourceFractionRemaining;
        private Auxiliary nonrenewableResourceUsageFactor;
        private Auxiliary perCapitaResourceUsageMultiplier;
        private Auxiliary fractionOfCapitalAllocatedToObtainingResources;
        private Auxiliary indexOfPersistentPollution;
        private Auxiliary persistentPollutionGenerationFactor;
        private Auxiliary persistentPollutionGeneratedByIndustrialOutput;
        private Auxiliary persistentPollutionGeneratedByAgriculturalOutput;
        private Auxiliary assimilationHalfLife;
        private Auxiliary fractionOfOutputInAgriculture;
        private Auxiliary fractionOfOutputInServices;
        private Auxiliary fractionOfOutputInIndustry;

        private Auxiliary population0To44;
        private Auxiliary population0To64;

        private Table fractionOfCapitalAllocatedToObtainingResourcesBefore;
        private Table fractionOfCapitalAllocatedToObtainingResourcesAfter;
        private Table assimilationHalfLifeMultiplier;

        private double nonrenewableResourcesInitialK = 1.0e12;  // resource units, used in eqns 129 and 133
        private const double nonrenewableResourcesInitialBase = 1.0e12;  // resource units, used in eqns 129 and 133

        public void SetInitialResources (double multiplier)
        {
            this.nonrenewableResourcesInitialK = nonrenewableResourcesInitialBase * multiplier;
            this.nonrenewableResources.ReInitialize(this.nonrenewableResourcesInitialK);
        }

        private void CreateOtherSectors()
        {
            #region Model Variables: NONRENEWABLE RESOURCE SECTOR

            this.sector = "Non-renewable Resources";
            this.subSector = string.Empty;


            this.nonrenewableResources =
                new Level(this, "nonrenewableResources", 129, "resource units", this.nonrenewableResourcesInitialK)
                {
                    UpdateFunction = delegate () { return nonrenewableResources.J + this.DeltaTime * (-nonrenewableResourceUsageRate.J); }
                };


            this.nonrenewableResourceUsageRate = new Rate(this, "nonrenewableResourceUsageRate", 130, "resource units per year")
            {
                UpdateFunction = delegate ()
                {
                    return population.K * perCapitaResourceUsageMultiplier.K * nonrenewableResourceUsageFactor.K;
                }
            };


            this.nonrenewableResourceUsageFactor = new Auxiliary(this, "nonrenewableResourceUsageFactor", 131, "dimensionless")
            {
                //nonrenewableResourceUsageFactor.before = 1;
                //nonrenewableResourceUsageFactor.after = 1;
                //nonrenewableResourceUsageFactor.updateFn = function() {
                //    return clip(this.after, this.before, t, policyYear);
                //}
                // Always one ??? Possibly a bug ? 
                UpdateFunction = delegate () { return Clip(1.0, 1.0, this.Time, WorldModel.PolicyYear); }
            };

            this.perCapitaResourceUsageMultiplier =
                new Table(this, "perCapitaResourceUsageMultiplier", 132, "resource units per person-year",
                    new double[] { 0, 0.85, 2.6, 4.4, 5.4, 6.2, 6.8, 7, 7 }, 0, 1600, 200)
                {
                    UpdateFunction = delegate () { return industrialOutputPerCapita.K; }
                };

            this.nonrenewableResourceFractionRemaining = new Auxiliary(this, "nonrenewableResourceFractionRemaining", 133, "dimensionless")
            {
                UpdateFunction = delegate () { return nonrenewableResources.K / nonrenewableResourcesInitialK; }
            };

            this.fractionOfCapitalAllocatedToObtainingResources =
                new Auxiliary(this, "fractionOfCapitalAllocatedToObtainingResources", 134, "dimensionless")
                {
                    UpdateFunction = delegate ()
                    {
                        return Clip(
                            fractionOfCapitalAllocatedToObtainingResourcesAfter.K,
                            fractionOfCapitalAllocatedToObtainingResourcesBefore.K,
                            this.Time, WorldModel.PolicyYear);
                    }
                };

            // BUG ??? The tables After and Before are identical 
            this.fractionOfCapitalAllocatedToObtainingResourcesBefore =
                new Table(
                    this, "fractionOfCapitalAllocatedToObtainingResourcesBefore", 135, "dimensionless",
                    new double[] { 1, 0.9, 0.7, 0.5, 0.2, 0.1, 0.05, 0.05, 0.05, 0.05, 0.05 }, 0, 1, 0.1)
                {
                    UpdateFunction = delegate () { return nonrenewableResourceFractionRemaining.K; }
                };

            this.fractionOfCapitalAllocatedToObtainingResourcesAfter =
                new Table(
                    this, "fractionOfCapitalAllocatedToObtainingResourcesAfter", 136, "dimensionless",
                    new double[] { 1, 0.9, 0.7, 0.5, 0.2, 0.1, 0.05, 0.05, 0.05, 0.05, 0.05 }, 0, 1, 0.1)
                {
                    UpdateFunction = delegate () { return nonrenewableResourceFractionRemaining.K; }
                };

            #endregion Model Variables: NONRENEWABLE RESOURCE SECTOR

            #region Model Variables: PERSISTENT POLLUTION SECTOR

            this.sector = "Persistent Pollution";
            this.subSector = string.Empty;

            this.persistentPollutionGenerationRate = new Rate(this, "persistentPollutionGenerationRate", 137, "pollution units per year")
            {
                UpdateFunction = delegate ()
                {
                    return
                        (persistentPollutionGeneratedByIndustrialOutput.K + persistentPollutionGeneratedByAgriculturalOutput.K) *
                         persistentPollutionGenerationFactor.K;
                }
            };

            this.persistentPollutionGenerationFactor = new Auxiliary(this, "persistentPollutionGenerationFactor", 138)
            {
                //persistentPollutionGenerationFactor.before = 1;
                //persistentPollutionGenerationFactor.after = 1;
                //persistentPollutionGenerationFactor.updateFn = function() {
                //    return clip(this.after, this.before, t, policyYear); }
                // Always one ??? Possibly a bug ? 
                UpdateFunction = delegate () { return Clip(1.0, 1.0, this.Time, WorldModel.PolicyYear); }
            };

            const double persistentPollutionGeneratedByIndustrialOutputFractionOfResourcesAsPersistentMaterial = 0.02;  // dimensionless
            const double persistentPollutionGeneratedByIndustrialOutputIndustrialMaterialsEmissionFactor = 0.1;  // dimensionless
            const double persistentPollutionGeneratedByIndustrialOutputIndustrialMaterialsToxicityIndex = 10;  // pollution units per resource unit  
            this.persistentPollutionGeneratedByIndustrialOutput =
                new Auxiliary(this, "persistentPollutionGeneratedByIndustrialOutput", 139, "pollution units per year")
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            perCapitaResourceUsageMultiplier.K * population.K *
                            persistentPollutionGeneratedByIndustrialOutputFractionOfResourcesAsPersistentMaterial *
                            persistentPollutionGeneratedByIndustrialOutputIndustrialMaterialsEmissionFactor *
                            persistentPollutionGeneratedByIndustrialOutputIndustrialMaterialsToxicityIndex;
                    }
                };


            const double persistentPollutionGeneratedByAgriculturalOutputFractionOfInputsAsPersistentMaterial = 0.001;  // dimensionless
            const double persistentPollutionGeneratedByAgriculturalOutputAgriculturalMaterialsToxicityIndex = 1;  // pollution units per dollar  
            this.persistentPollutionGeneratedByAgriculturalOutput =
                new Auxiliary(this, "persistentPollutionGeneratedByAgriculturalOutput", 140, "pollution units per year")
                {
                    UpdateFunction = delegate ()
                    {
                        return
                            agriculturalInputsPerHectare.K * arableLand.K *
                            persistentPollutionGeneratedByAgriculturalOutputFractionOfInputsAsPersistentMaterial *
                            persistentPollutionGeneratedByAgriculturalOutputAgriculturalMaterialsToxicityIndex;
                    }
                };

            const double persistentPollutionTransmissionDelayK = 20.0; // years, used in eqn 141  
            // Bug ? WHY ??? 
            // persistenPollutionAppearanceRate.qType = "Rate";
            // rateArray.push(auxArray.pop());   // put this among the Rates, not the Auxes
            // Bug : See if we can rename easily 
            this.persistenPollutionAppearanceRate =
                new Delay(
                    this, "persistenPollutionAppearanceRate", 141, "pollution units per year",
                    persistentPollutionTransmissionDelayK, "persistentPollutionGenerationRate");

            this.persistentPollution = new Level(this, "persistentPollution", 142, "pollution units", 2.5e7)
            {
                UpdateFunction = delegate ()
                {
                    return
                        persistentPollution.J + this.DeltaTime * (persistenPollutionAppearanceRate.J - persistenPollutionAssimilationRate.J);
                }
            };

            const double indexOfPersistentPollutionPollutionValueIn1970 = 1.36e8; // pollution units, used in eqn 143
            this.indexOfPersistentPollution = new Auxiliary(this, "indexOfPersistentPollution", 143)
            {
                UpdateFunction = delegate () { return persistentPollution.K / indexOfPersistentPollutionPollutionValueIn1970; }
            };



            // Naming Bug : See if we can rename easily 
            this.persistenPollutionAssimilationRate = new Rate(this, "persistenPollutionAssimilationRate", 144, "pollution units per year")
            {
                UpdateFunction = delegate () { return persistentPollution.K / (assimilationHalfLife.K * 1.4); }
            };


            this.assimilationHalfLifeMultiplier =
                new Table(this, "assimilationHalfLifeMultiplier", 145, "dimensionless",
                    new double[] { 1, 11, 21, 31, 41 }, 1, 1001, 250)
                {
                    UpdateFunction = delegate () { return indexOfPersistentPollution.K; }
                };

            const double assimilationHalfLifeValueIn1970 = 1.5; // years
            this.assimilationHalfLife = new Auxiliary(this, "assimilationHalfLife", 146, "years")
            {
                UpdateFunction = delegate () { return assimilationHalfLifeMultiplier.K * assimilationHalfLifeValueIn1970; }
            };

            #endregion Model Variables: PERSISTENT POLLUTION SECTOR

            #region Model Variables: SUPPLEMENTARY EQUATIONS

            this.sector = "Supplementary Equations";
            this.subSector = string.Empty;

            this.fractionOfOutputInAgriculture = new Auxiliary(this, "fractionOfOutputInAgriculture", 147)
            {
                UpdateFunction = delegate ()
                {
                    return 0.22 * food.K / (0.22 * food.K + serviceOutput.K + industrialOutput.K);
                }
            };

            this.fractionOfOutputInIndustry = new Auxiliary(this, "fractionOfOutputInIndustry", 148)
            {
                UpdateFunction = delegate ()
                {
                    return industrialOutput.K / (0.22 * food.K + serviceOutput.K + industrialOutput.K);
                }
            };

            this.fractionOfOutputInServices = new Auxiliary(this, "fractionOfOutputInServices", 149)
            {
                UpdateFunction = delegate ()
                {
                    return serviceOutput.K / (0.22 * food.K + serviceOutput.K + industrialOutput.K);
                }
            };

            #endregion Model Variables: SUPPLEMENTARY EQUATIONS

            #region Model Variables: NEW EQUATIONS

            this.population0To44 = new Auxiliary(this, "population0To44", 150, "persons")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return population0To14.K + population15To44.K;
                }
            };

            this.population0To64 = new Auxiliary(this, "population0To64", 151, "persons")
            {
                CannotBeNegative = true,
                CannotBeZero = true,
                UpdateFunction = delegate ()
                {
                    return population0To14.K + population15To44.K + population45To64.K;
                }
            };

            #endregion Model Variables: NEW EQUATIONS

        }
    }
}
