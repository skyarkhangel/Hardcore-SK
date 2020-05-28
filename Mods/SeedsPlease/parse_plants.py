#!/bin/env python

import os
import sys
from lxml import etree

if len(sys.argv) < 2:
    exit(1)

seeds = etree.Element("Defs")
recipes = etree.Element("Defs")

prefixes = ['VG_Plant', 'VGP_', 'Plant_', 'WildPlant', 'Wild', 'Plant', 'tree', 'Tree']

for file in sys.argv[1:]:
    tree = etree.parse(file)
    root = tree.getroot()
    
    thingDefs = root.findall('ThingDef');
    
    basename = ' ' + os.path.basename(file) + ' '
    seeds.append(etree.Comment(basename))
    recipes.append(etree.Comment(basename))
    
    print('\n', basename, len(thingDefs))
    
    for child in thingDefs:
        defName = child.find('defName')
        if defName is None:
            continue
        
        name = defName.text
        
        for w in prefixes:
          name = name.replace(w, '')
          
        name = name.capitalize()

        plant = child.find('plant')
        if plant is None:
            continue
          
        parentName = child.get('ParentName')
        if parentName is None:
            continue
          
        plantYield = plant.find('harvestYield');
        
        if plantYield is None:
            continue
        else:
            plantYield = plantYield.text
        
        texture = 'Things/Item/Seeds/' + name
        hasTexture = os.path.exists('Textures/' + texture)
        
        yieldCount = max(int(round(int(plantYield)/3)), 4)
        
        print('\t', name, parentName, plantYield, str(yieldCount), hasTexture)

        sthingDef = etree.SubElement(seeds, 'SeedsPlease.SeedDef')
        sthingDef.set('ParentName', 'SeedBase')
        sdefName = etree.SubElement(sthingDef, 'defName')
        sdefName.text = 'Seed_' + name;
        slabel = etree.SubElement(sthingDef, 'label')
        slabel.text = name.lower() + ' seeds'
        splant = etree.SubElement(sthingDef, 'plant')
        splant.text = defName.text
        if False:
            sstatBases = etree.SubElement(sthingDef, 'statBases')
            sMarketValue = etree.SubElement(sstatBases, 'MarketValue')
            sMarketValue.text = '0'
            sseed = etree.SubElement(sthingDef, 'seed')
            sharvestFactor = etree.SubElement(sseed, 'harvestFactor')
            sharvestFactor.text = '1.0'
            sseedFactor = etree.SubElement(sseed, 'seedFactor')
            sseedFactor.text = '1.0'
            sbaseChance = etree.SubElement(sseed, 'baseChance')
            sbaseChance.text = '1.0'
            sextraChance = etree.SubElement(sseed, 'extraChance')
            sextraChance.text = '0.1'
        if hasTexture:
            sTexture = etree.XML('<graphicData><texPath>' + texture + '</texPath></graphicData>')
            sthingDef.append(sTexture)
        
        harvestedThingDef = plant.find('harvestedThingDef')
        if harvestedThingDef is None:
            continue
        
        rthingDef = etree.SubElement(recipes, 'RecipeDef')
        rthingDef.set('ParentName', 'ExtractSeed')
        rdefName = etree.SubElement(rthingDef, 'defName')
        rdefName.text = 'ExtractSeed_' + name;
        rlabel = etree.SubElement(rthingDef, 'label')
        rlabel.text = 'extract ' + name.lower() + ' seeds'
        rdesc = etree.SubElement(rthingDef, 'description')
        rdesc.text = 'Extract seeds from ' + harvestedThingDef.text.replace('Raw', '') + '.'
        ringredients = etree.XML('<ingredients><li><filter><thingDefs><li>' + harvestedThingDef.text + '</li></thingDefs></filter><count>' + str(yieldCount) + '</count></li></ingredients>')
        rthingDef.append(ringredients)
        rfixedIngredientsFilter = etree.XML('<fixedIngredientFilter><thingDefs><li>' + harvestedThingDef.text + '</li></thingDefs></fixedIngredientFilter>')
        rthingDef.append(rfixedIngredientsFilter);
        rproducts = etree.SubElement(rthingDef, 'products')
        rproduct = etree.SubElement(rproducts, sdefName.text)
        rproduct.text = '3'
        
        research = plant.find('sowResearchPrerequisites')
        if research is None or not len(research):
            continue
        rresearch = etree.SubElement(rthingDef, 'researchPrerequisite')
        rresearch.text = research[0].text

etree.ElementTree(seeds).write('Items_Seeds.xml', pretty_print=True)
etree.ElementTree(recipes).write('Recipes_Plants.xml', pretty_print=True)
