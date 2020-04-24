﻿using FrontEnd.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using FrontEnd.View.Controller;
using Shared.Global;
using FrontEnd.Controller.Prompts;
using FrontEnd.Controller.Parts;
using System.Linq;
using Backend.Model;
using Shared.Model;

namespace FrontEnd.App.Views
{
    /// <summary>
    /// Partial View Controller
    /// </summary>
    public partial class MainApp : Form
    {
        private ControlsAccess _controls;
        private EventViewController _events;

        /// <summary>
        /// Constructor for Partial View Controller
        /// </summary>
        public MainApp()
        {
            InitializeComponent();
            SetupControls();
        }

        private void SetupControls()
        {
            _controls = new ControlsAccess();
            _events = new EventViewController(_controls);

            Tag = _controls.AddForm(this);
            string tag = Tag as string;

            Time.Tag = _controls.Add(tag, new LabelController(Time));
            Date.Tag = _controls.Add(tag, new LabelController(Date));
            LatestEvent.Tag = _controls.Add(tag, new LabelController(LatestEvent));
            ExpStartDate.Tag = _controls.Add(tag, new LabelController(ExpStartDate));
            ExpStartTime.Tag = _controls.Add(tag, new LabelController(ExpStartTime));
            ExpEndDate.Tag = _controls.Add(tag, new LabelController(ExpEndDate));
            ExpEndTime.Tag = _controls.Add(tag, new LabelController(ExpEndTime));
            Title.Tag = _controls.Add(tag, new LabelController(Title));
            EventStatus.Tag = _controls.Add(tag, new LabelController(EventStatus));
            CompletedDate.Tag = _controls.Add(tag, new LabelController(CompletedDate));
            CreateDate.Tag = _controls.Add(tag, new LabelController(CreateDate));
            Comment.Tag = _controls.Add(tag, new TextBoxController(Comment));
            Calendar.Tag = _controls.Add(tag, new CalendarController(Calendar));
            TodaysEvents.Tag = _controls.Add(tag, new ListBoxController(TodaysEvents));

            Todays_Events.SetMembers("Title", "Id");
            Time.Text = TimeAndDateUtility.GetCurrentTimeString();
            Date.Text = TimeAndDateUtility.GetCurrentDateString();

            UpdateTodaysEvents();

            Bitmap bit = Resources.ChillSched;
            IntPtr pIcon = bit.GetHicon();
            Icon = Icon.FromHandle(pIcon);
            DateTimeIcon.Icon = Icon;
        }

        private void TimeTicker_Tick(object sender, EventArgs e)
        {
            User_Time.GetControl().Text = TimeAndDateUtility.GetCurrentTimeString();
        }

        private void DateTicker_Tick(object sender, EventArgs e)
        {
            User_Date.GetControl().Text = TimeAndDateUtility.GetCurrentDateString();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void DateTimeIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
            e.Cancel = true;
        }

        private void TodaysEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get events to show in listbox
            //Highlight events past date and time
            try
            {
                string id = ((SavedEvent)Todays_Events.SelectedIndex()).Id;
                SavedEvent @event = _events.GetEvent(id);

                Date currDate = TimeAndDateUtility.GetCurrentDate();
                Time currTime = TimeAndDateUtility.GetCurrentTime();

                Date eventStartDate = @event.ActivationDate;
                Time eventStartTime = @event.ActivationTime;

                SetEventDetails(@event);
                ToggleButtons(true, TimeAndDateUtility.IsBeforeRange(eventStartDate, eventStartTime, currDate, currTime));
            }
            catch (Exception)
            {
                // Log
            }
        }

        private void Calendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            User_Calendar.GetControl().BoldedDates = new[] { User_Calendar.GetControl().SelectionStart };
            ClearEventDetails();
            UpdateTodaysEvents();
            ToggleButtons();
        }

        private void CreateEvent_Click(object sender, EventArgs e)
        {
            if (_events.Add())
            {
                ClearEventDetails();
                UpdateTodaysEvents();
                ToggleButtons();
            }
        }

        private void EditEvent_Click(object sender, EventArgs e)
        {
            string id = ((SavedEvent)Todays_Events.SelectedIndex()).Id;
            if (_events.Update(id))
            {
                ClearEventDetails();
                UpdateTodaysEvents();
                ToggleButtons();
            }
        }

        private void ToggleStatus_Click(object sender, EventArgs e)
        {
            string id = ((SavedEvent)Todays_Events.SelectedIndex()).Id;
            if (_events.ToggleStatus(id))
            {
                ClearEventDetails();
                UpdateTodaysEvents();
                ToggleButtons();
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {

        }

        #region Helpers

        private void SetEventDetails(SavedEvent @event)
        {
            Date currDate = TimeAndDateUtility.GetCurrentDate();
            Time currTime = TimeAndDateUtility.GetCurrentTime();

            Date eventStartDate = @event.ActivationDate;
            Time eventStartTime = @event.ActivationTime;

            Date eventEndDate = @event.DeactivationDate;
            Time eventEndTime = @event.DeactivationTime;

            Exp_Start_Date.SetText(TimeAndDateUtility.ConvertDate_String(@event.ActivationDate, true));
            Exp_Start_Time.SetText(TimeAndDateUtility.ConvertTime_String(@event.ActivationTime));
            Exp_End_Date.SetText(TimeAndDateUtility.ConvertDate_String(@event.DeactivationDate, true));
            Exp_End_Time.SetText(TimeAndDateUtility.ConvertTime_String(@event.DeactivationTime));
            Created_Date.SetText(
                $"{TimeAndDateUtility.ConvertDate_String(@event.DateCreated, true)} {TimeAndDateUtility.ConvertTime_String(@event.TimeCreated)}");

            if (@event.Completed && @event.DateCompleted != null && @event.TimeCompleted != null)
            {
                Completion_Date.SetText(
                    $"{TimeAndDateUtility.ConvertDate_String(@event.DateCompleted, true)} {TimeAndDateUtility.ConvertTime_String(@event.TimeCompleted)}");
            }

            Event_Status.SetText(
                @event.Completed ? "Complete"
                    : TimeAndDateUtility.IsBeforeRange(eventStartDate, eventStartTime, currDate, currTime) ?
                        "Upcoming"
                        : TimeAndDateUtility.IsWithinRange(eventStartDate, eventStartTime, currDate, currTime, eventEndDate, eventEndTime) ?
                            "In Progress" : "Overdue");
            Event_Status.SetBackColor(
                @event.Completed ? Color.DarkGreen
                    : TimeAndDateUtility.IsBeforeRange(eventStartDate, eventStartTime, currDate, currTime) ?
                        Color.DarkGray
                        : TimeAndDateUtility.IsWithinRange(eventStartDate, eventStartTime, currDate, currTime, eventEndDate, eventEndTime) ?
                            Color.DarkBlue : Color.DarkRed);

            User_Title.SetText(@event.Title);

            User_Comment.SetText(@event.Comment);
        }

        private void ToggleButtons(bool enable = false, bool isBefore = false)
        {
            EditEvent.Enabled = enable;
            RemoveButton.Enabled = enable;
            ToggleStatus.Enabled = !isBefore && enable;
            ToggleStatus.BackColor = ToggleStatus.Enabled ? Color.WhiteSmoke : Color.Transparent;
        }

        private void ClearEventDetails()
        {
            const string dash = "-";
            const string no_comment = "No comment";

            Exp_Start_Date.SetText(dash);
            Exp_Start_Time.SetText(dash);
            Exp_End_Date.SetText(dash);
            Exp_End_Time.SetText(dash);
            Created_Date.SetText(dash);
            Completion_Date.SetText(dash);
            Event_Status.SetText(dash);
            Event_Status.SetBackColor(Color.DarkGray);

            User_Title.SetText(dash);

            User_Comment.SetText(no_comment);
        }

        private void UpdateTodaysEvents()
        {
            DateTime selectedDate = User_Calendar.GetControl().SelectionRange.Start;
            User_Calendar.GetControl().BoldedDates = new[] { selectedDate };
            Date date = TimeAndDateUtility.ConvertDate_Date(selectedDate);
            object[] eventList = _events.GetAll(date).OrderBy(x => x.Title).Select(x => (object)x).Distinct().ToArray();

            Todays_Events.Update(eventList);
        }

        #endregion Helpers

        #region Controllers [Only use once view is initialized]

        #region [ ListBoxes ]

        private ListBoxController Todays_Events => (ListBoxController)_controls.Get(Tag as string, TodaysEvents.Tag as string);

        #endregion

        #region [ TextBoxes ]

        private TextBoxController User_Comment => (TextBoxController)_controls.Get(Tag as string, Comment.Tag as string);

        #endregion

        #region [ Date ]

        private CalendarController User_Calendar => (CalendarController)_controls.Get(Tag as string, Calendar.Tag as string);

        #endregion

        #region [ Labels ]

        private LabelController User_Time => (LabelController)_controls.Get(Tag as string, Time.Tag as string);

        private LabelController User_Date => (LabelController)_controls.Get(Tag as string, Date.Tag as string);

        private LabelController Exp_Start_Date => (LabelController)_controls.Get(Tag as string, ExpStartDate.Tag as string);

        private LabelController Exp_Start_Time => (LabelController)_controls.Get(Tag as string, ExpStartTime.Tag as string);

        private LabelController Exp_End_Date => (LabelController)_controls.Get(Tag as string, ExpEndDate.Tag as string);

        private LabelController Exp_End_Time => (LabelController)_controls.Get(Tag as string, ExpEndTime.Tag as string);

        private LabelController User_Title => (LabelController)_controls.Get(Tag as string, Title.Tag as string);

        private LabelController Event_Status => (LabelController)_controls.Get(Tag as string, EventStatus.Tag as string);

        private LabelController Completion_Date => (LabelController)_controls.Get(Tag as string, CompletedDate.Tag as string);

        private LabelController Created_Date => (LabelController)_controls.Get(Tag as string, CreateDate.Tag as string);

        #endregion

        #endregion
    }
}