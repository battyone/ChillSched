﻿using System;
using System.Windows.Forms;
using FrontEnd.View.Controller;
using Backend.Model;
using FrontEnd.Controller.Parts;
using FrontEnd.Controller.Prompts;

namespace FrontEnd.App.Parts
{
    /// <summary>
    /// Partial class for Message Selection View
    /// </summary>
    public partial class MessagesView : UserControl
    {
        private string _parentId;
        private MessageViewController _controller;
        private ControlsAccess _controls;

        /// <summary>
        /// The created/edited message data
        /// </summary>
        public AppMessage Results { get; private set; }

        /// <summary>
        /// Constructor for the Partial View Controllers
        /// </summary>
        public MessagesView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets up the controls access for Message Select View
        /// </summary>
        /// <param name="parentId">The id of the parent</param>
        /// <param name="controls">The controls</param>
        /// <param name="controller">The controller to use</param>
        public void SetControls(string parentId, ControlsAccess controls, MessageViewController controller)
        {
            _controls = controls;
            _parentId = parentId;
            _controller = controller;
            Setup();
        }

        public void UpdateMessagesView()
        {
            SearchTB.SetText(string.Empty);
            UpdateMessages();
            ToggleButtons(false, "-");
        }

        /// <summary>
        /// Sets the title of the Group Box
        /// </summary>
        /// <param name="title">The title to set</param>
        public void SetTitle(string title)
        {
            MessagesGB.Text = title;
        }

        private void Setup()
        {
            SearchBox.Tag = _controls.Add(_parentId, new TextBoxController(SearchBox));
            MessageListBox.Tag = _controls.Add(_parentId, new ListBoxController(MessageListBox));

            MessagesLB.SetMembers("Title", "Id");
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchButton.PerformClick();
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            UpdateMessages();
            ToggleButtons(false, "-");
        }

        private void MessageListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string id = ((AppMessage)MessagesLB.SelectedIndex())?.Id;

                if (!string.IsNullOrEmpty(id))
                {
                    AppMessage message = _controller.GetMessage(id);
                    ToggleButtons(true, message.Show ? "Hide" : "Show");
                }
            }
            catch (Exception)
            {
                //Something happened
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (_controller.Add())
            {
                ClearMessageDetails();
                ToggleButtons(false, "-");
                UpdateMessages();
                SearchTB.SetText(string.Empty);
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            try
            {
                string id = ((AppMessage)MessagesLB.SelectedIndex())?.Id;

                if (_controller.Update(null))
                {
                    ClearMessageDetails();
                    ToggleButtons(false, "-");
                    UpdateMessages();
                    SearchTB.SetText(string.Empty);
                }
            }
            catch (Exception)
            {
                //Something happened
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            try
            {
                string id = ((AppMessage)MessagesLB.SelectedIndex())?.Id;

                if (!string.IsNullOrEmpty(id) && _controller.Remove(id))
                {
                    ClearMessageDetails();
                    ToggleButtons(false, "-");
                    UpdateMessages();
                    SearchTB.SetText(string.Empty);
                }
            }
            catch(Exception)
            {
                //Something happened
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            string id = ((AppMessage)MessagesLB.SelectedIndex())?.Id;

            if (!string.IsNullOrEmpty(id) && _controller.ToggleShow(id))
            {
                ClearMessageDetails();
                ToggleButtons(false, "-");
                UpdateMessages();
            }
        }

        #region Helpers

        private void UpdateMessages()
        {
            if (string.IsNullOrEmpty(SearchTB.Text))
            {
                object[] messages = _controller.GetAll();
                MessagesLB.Update(messages);
            }
            else
            {
                object[] messages = _controller.GetAll(SearchTB.Text);
                MessagesLB.Update(messages);
            }
        }

        private void ToggleButtons(bool enable, string toggleText)
        {
            EditButton.Enabled = enable;
            RemoveButton.Enabled = enable;
            ToggleButton.Enabled = enable;
            ToggleButton.Text = toggleText ?? ToggleButton.Text;
        }

        private void ClearMessageDetails()
        {
            //throw new NotImplementedException();
        }

        #endregion Helpers

        #region Cleanup

        public void Cleanup()
        {
            _controls.Remove(_parentId, SearchTB.GetId());
            _controls.Remove(_parentId, MessagesLB.GetId());
        }

        #endregion Cleanup

        #region Controllers [Only use once view is initialized]

        #region [ TextBoxes ]

        private TextBoxController SearchTB => (TextBoxController)_controls.Get(_parentId, SearchBox.Tag as string);

        #endregion

        #region [ ListBoxes ]

        private ListBoxController MessagesLB => (ListBoxController)_controls.Get(_parentId, MessageListBox.Tag as string);

        #endregion

        #endregion
    }
}