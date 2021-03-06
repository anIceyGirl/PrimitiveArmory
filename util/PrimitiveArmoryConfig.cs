﻿using OptionalUI;
using Partiality;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrimitiveArmory
{
    public class PrimitiveArmoryConfig : OptionInterface
    {
        public PrimitiveArmoryConfig() : base(Main.instance)
        {
            mod = Main.instance;
        }

        public override void Initialize()
        {
            try
            {
                base.Initialize();

                if (Main.EnumExt)
                {
                    Debug.Log("Initializing tab listing...");
                    this.Tabs = new OpTab[5];

                    Debug.Log("Initializing individual tabs...");
                    this.Tabs[0] = new OpTab("Toggle Items");
                    this.Tabs[1] = new OpTab("Spawning Options");
                    this.Tabs[2] = new OpTab("Item Config");
                    this.Tabs[3] = new OpTab("Changelog/Credits");
                    this.Tabs[4] = new OpTab("Known Issues");

                    Debug.Log("Setting splash text...");
                    string splash = RandomTabHeader();

                    Debug.Log("Setting tab text...");
                    string nothingHereYet = "There's nothing here yet...\n:(";
                    string knownIssues = "Known issues:"
                        + "\nKilling creatures with the club do not register as a kill in Arena mode.";

                    for (int i = 0; i < this.Tabs.Length; i++)
                    {
                        Debug.Log("Initializing Tab " + this.Tabs[i].name);
                        AddHeader(this.Tabs, i, splash);
                    }

                    ToggleItemsTab(this.Tabs, 0);
                    BigTextTab(this.Tabs, 1, nothingHereYet, FLabelAlignment.Center);
                    BigTextTab(this.Tabs, 2, nothingHereYet, FLabelAlignment.Center);
                    BigTextTab(this.Tabs, 3, Main.credits + "\n\n" + Main.changelog);
                    BigTextTab(this.Tabs, 4, knownIssues);
                }
                else
                {
                    throw new PrimitiveArmory.ModDependencyNotFound();
                }
            }
            catch (Exception e)
            {
                this.Tabs = new OpTab[1];

                this.Tabs[0] = new OpTab("Error");

                string errorLog;

                switch (e.GetType().ToString())
                {
                    case "PrimitiveArmory.ModDependencyNotFound":
                        errorLog = "EnumExtender was not found!\nBecause of this, PrimitiveArmory is unable to add new items properly.\nPlease install EnumExtender or remove PrimitiveArmory.\n\nError: \n" + e;
                        Debug.LogError("EnumExtender not found! PrimitiveArmory cannot add new items.");
                        break;
                    default:
                        errorLog = "An unknown error occured.\n\nError: \n" + e;
                        break;
                }

                AddHeader(this.Tabs, 0, RandomTabHeader(true));
                BigTextTab(this.Tabs, 0, errorLog, FLabelAlignment.Center);
            }
        }

        public void AddHeader(OpTab[] TabList, int tabNum, string headerTxt)
        {

            OpLabel headerID = new OpLabel(new Vector2(62.5f, 530f), new Vector2(475f, 50f), "Primitive Armory".ToUpper(), FLabelAlignment.Center, true);
            OpLabel headerDescID = new OpLabel(new Vector2(62.5f, 520f), new Vector2(475f, 25f), headerTxt, FLabelAlignment.Center, false);
            OpLabel versionID = new OpLabel(new Vector2(62.5f, 525f), new Vector2(225f, 40f), "Version: " + Main.instance.Version, FLabelAlignment.Left, false);
            OpLabel authorID = new OpLabel(new Vector2(312.5f, 525f), new Vector2(225f, 40f), "Author: " + Main.instance.author, FLabelAlignment.Right, false);
            OpRect optionsContainer = new OpRect(new Vector2(40f, 12f), new Vector2(525f, 500f));
            OpRect headerContainer = new OpRect(new Vector2(40f, 512f), new Vector2(525f, 65f));

            TabList[tabNum].AddItems(headerID);
            TabList[tabNum].AddItems(headerDescID);
            TabList[tabNum].AddItems(versionID);
            TabList[tabNum].AddItems(authorID);
            TabList[tabNum].AddItems(optionsContainer);
            TabList[tabNum].AddItems(headerContainer);
        }

        public void BigTextTab(OpTab[] TabList, int tabNum, string txt, FLabelAlignment alignment = FLabelAlignment.Left)
        {
            OpRect TextBoxContainer = new OpRect(new Vector2(65f, 37f), new Vector2(475f, 450f));
            OpLabel TextBoxID = new OpLabel(new Vector2(90f, 60f), new Vector2(424f, 402f), txt, alignment, false)
            {
                autoWrap = true
            };

            TabList[tabNum].AddItems(TextBoxContainer);
            TabList[tabNum].AddItems(TextBoxID);
        }

        public override bool Configuable() { return true; }

        public string RandomTabHeader(bool isError = false)
        {                      
            if (!isError)
            {
                Dictionary<int, string> specialDates = new Dictionary<int, string>()
                {
                    { 312, "Happy birthday, Icey!" },
                    { 304, "Happy birthday, Ross!" },
                    { 135, "Happy birthday, Sedric!" },
                    // { 80, "Happy birthday to that guy who suggested mosquitos!" }
                };
                int currentDate = DateTime.Now.DayOfYear;

                if (specialDates.ContainsKey(currentDate))
                {
                    return specialDates[currentDate];
                }
                

                string[] randomDesc = new string[]
                {
                    "New items and weapons for Rain World!",
                    "Woo, RainDB!",
                    "Where there is not light, there can spider!",
                    "Singing in the ra-- AAAAAAAAAAAAAAAAAAAAA",
                    "This isn't Shoreline, this is a bathtub!",
                    "haha slugcat go bonk",
                    "I have a suggestion.",
                    "What fate a slugcat?",
                    "Made in C#!",
                    "Parry this, you filthy casual!",
                    "The signals... what do they mean?!",
                    "Slightly morbid!",
                    "Alpha version!",
                    "Pre-beta!",
                    "You brought a club to a spear fight?"
                };

                return randomDesc[UnityEngine.Random.Range(0, randomDesc.Length - 1)];
            }
            else
            {
                string[] randomDesc = new string[]
                {
                    "Uh... did I do that?",
                    "You're laughing. The config screen had an error, and you're laughing.",
                    "I'm trying my hardest, okay.",
                    "Look at what you've done.",
                    "*sounds of slugcats smashing keyboards with intent*"
                };

                return randomDesc[UnityEngine.Random.Range(0, randomDesc.Length - 1)];
            }
        }

        public void ToggleItemsTab(OpTab[] TabList, int tabNum)
        {

            OpRect toggleContainer = new OpRect(new Vector2(65f, 185f), new Vector2(200f, 302f));
            OpRect descContainer = new OpRect(new Vector2(263f, 185f), new Vector2(277f, 302f));
            OpRect specialOptionsContainer = new OpRect(new Vector2(65f, 37f), new Vector2(475f, 150f));

            TabList[tabNum].AddItems(toggleContainer);
            TabList[tabNum].AddItems(descContainer);
            TabList[tabNum].AddItems(specialOptionsContainer);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
        }
    }
}
