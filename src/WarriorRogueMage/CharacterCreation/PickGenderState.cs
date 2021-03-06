﻿//-----------------------------------------------------------------------------
// <copyright file="PickGenderState.cs" company="WheelMUD Development Team">
//   Copyright (c) WheelMUD Development Team.  See LICENSE.txt.  This file is 
//   subject to the Microsoft Public License.  All other rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace WarriorRogueMage.CharacterCreation
{
    using System;
    using System.Linq;
    using System.Text;
    using WheelMUD.ConnectionStates;
    using WheelMUD.Core;

    /// <summary>The character creation step where the player will pick their gender.</summary>
    public class PickGenderState : CharacterCreationSubState
    {
        private GameGender selectedGender;

        /// <summary>Initializes a new instance of the <see cref="PickGenderState"/> class.</summary>
        /// <param name="session">The session.</param>
        public PickGenderState(Session session)
            : base(session)
        {
            RefreshScreen(false);
        }

        /// <summary>ProcessInput is used to receive the user input during this state.</summary>
        /// <param name="command">The command text to be processed.</param>
        public override void ProcessInput(string command)
        {
            if (!string.IsNullOrEmpty(command) && SetGender(command))
            {
                ProcessDone();
            }
            else if (!HandleCommand(command))
            {
                WrmChargenCommon.SendErrorMessage(Session, "Invalid command. Please select a gender.");
            }
        }

        public override string BuildPrompt()
        {
            return string.Format("Select the character's gender.{0}>", Environment.NewLine);
        }

        private bool HandleCommand(string command)
        {
            // Special commands like "help <gender>" could be implemented here, to describe the gender
            // dynamically against the gender class properties, etc.
            return false;
        }

        private bool SetGender(string specifiedGender)
        {
            // Support strings of format "select m" and "m" by ignoring "select " from the input.
            string currentGender = specifiedGender.Replace("select ", string.Empty);

            selectedGender = (from g in GameSystemController.Instance.GameGenders
                              where g.Name.Equals(currentGender, StringComparison.CurrentCultureIgnoreCase) ||
                                    g.Abbreviation.Equals(currentGender, StringComparison.CurrentCultureIgnoreCase)
                              select g).FirstOrDefault();
            if (selectedGender == null)
            {
                WrmChargenCommon.SendErrorMessage(Session, string.Format("'{0}' is an invalid gender selection.", currentGender));
                RefreshScreen();
            }

            return selectedGender != null;
        }

        private void ProcessDone()
        {
            var playerBehavior = Session.Thing.Behaviors.FindFirst<PlayerBehavior>();
            playerBehavior.Gender = selectedGender;
            string doneMessage = string.Format("<%green%>The chosen gender is {0}.<%n%>" + Environment.NewLine, selectedGender.Name);
            Session.Write(doneMessage, false);

            // Proceed to the next step.
            StateMachine.HandleNextStep(this, StepStatus.Success);
        }

        private void RefreshScreen(bool sendPrompt = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("You have the following gender choices:");

            foreach (var gender in GameSystemController.Instance.GameGenders)
            {
                sb.AppendLine(gender.Name);
            }

            sb.AppendLine();
            sb.AppendLine("<%yellow%>===============================================================");
            sb.AppendLine("Type your gender selection.");
            sb.AppendLine("===============================================================<%n%>");

            Session.Write(sb.ToString(), sendPrompt);
        }
    }
}