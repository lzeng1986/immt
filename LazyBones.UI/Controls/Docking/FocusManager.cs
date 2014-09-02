using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LazyBones.Win32;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 用于管理焦点
    /// </summary>
    class FocusManager : Component, IContentFocusManager, IFocusManager
    {
        // Use a static instance of the windows hook to prevent stack overflows in the windows kernel.
        [ThreadStatic]
        static LocalWinHook localWinHook;
        IDockContent lastActiveContent = null;
        IDockContent contentActivating = null;
        List<IDockContent> listContent = new List<IDockContent>();
        bool inRefreshActiveWindow = false;
        readonly DockPanel dockPanel;
        public FocusManager(DockPanel dockPanel)
        {
            this.dockPanel = dockPanel;

            if (localWinHook == null)
            {
                localWinHook = new LocalWinHook(HookType.WH_CALLWNDPROCRET);
                localWinHook.Install();
            }

            localWinHook.HookInvoked += HookEventHandler;
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                localWinHook.HookInvoked -= HookEventHandler;
                disposed = true;
            }

            base.Dispose(disposing);
        }

        public void Activate(IDockContent content)
        {
            if (IsFocusTrackingSuspended)
            {
                contentActivating = content;
                return;
            }

            if (content == null)
                return;
            var handler = content.Handler;
            if (handler.Form.IsDisposed)
                return;
            if (ContentContains(content, handler.ActiveWindowHandle))
            {
                User32.SetFocus(handler.ActiveWindowHandle);
            }

            if (handler.Form.ContainsFocus)
                return;

            if (handler.Form.SelectNextControl(handler.Form.ActiveControl, true, true, true, true))
                return;
            Win32.User32.SetFocus(handler.Form.Handle);
        }
        
        public void AddToList(IDockContent content)
        {
            if (listContent.Contains(content) || IsInActiveList(content))
                return;

            listContent.Add(content);
        }

        public void RemoveFromList(IDockContent content)
        {
            if (IsInActiveList(content))
                RemoveFromActiveList(content);
            if (listContent.Contains(content))
                listContent.Remove(content);
        }

        bool IsInActiveList(IDockContent content)
        {
            return content.Handler.NextActiveContent != null || lastActiveContent == content;
        }

        void AddLastToActiveList(IDockContent content)
        {
            if (lastActiveContent == content)
                return;

            DockContentHandler handler = content.Handler;

            if (IsInActiveList(content))
                RemoveFromActiveList(content);

            handler.PreviousActiveContent = lastActiveContent;
            handler.NextActiveContent = null;
            lastActiveContent = content;
            if (lastActiveContent != null)
                lastActiveContent.Handler.NextActiveContent = lastActiveContent;
        }

        void RemoveFromActiveList(IDockContent content)
        {
            if (lastActiveContent == content)
                lastActiveContent = content.Handler.PreviousActiveContent;

            var prev = content.Handler.PreviousActiveContent;
            var next = content.Handler.NextActiveContent;
            if (prev != null)
                prev.Handler.NextActiveContent = next;
            if (next != null)
                next.Handler.PreviousActiveContent = prev;

            content.Handler.PreviousActiveContent = null;
            content.Handler.NextActiveContent = null;
        }
        public void GiveUpFocus(IDockContent content)
        {
            if (content == null)
                return;
            var handler = content.Handler;
            if (!handler.Form.ContainsFocus)
                return;

            if (IsFocusTrackingSuspended)
                dockPanel.dummyControl.Focus();

            if (lastActiveContent == content)
            {
                IDockContent prev = handler.PreviousActiveContent;
                if (prev != null)
                    Activate(prev);
                else if (listContent.Count > 0)
                    Activate(listContent.Last());
            }
            else if (lastActiveContent != null)
                Activate(lastActiveContent);
            else if (listContent.Count > 0)
                Activate(listContent.Last());
        }

        static bool ContentContains(IDockContent content, IntPtr hWnd)
        {
            var control = Control.FromChildHandle(hWnd);
            if (control != null)
                return control.FindForm() == content.Handler.Form;
            return false;
        }

        private int suspendFocusCount = 0;
        public void SuspendFocus()
        {
            suspendFocusCount++;
            localWinHook.HookInvoked -= HookEventHandler;
        }

        public void ResumeFocus()
        {
            if (suspendFocusCount > 0)
                suspendFocusCount--;

            if (suspendFocusCount == 0)
            {
                if (contentActivating != null)
                {
                    Activate(contentActivating);
                    contentActivating = null;
                }
                localWinHook.HookInvoked += HookEventHandler;
                if (!inRefreshActiveWindow)
                    RefreshActiveWindow();
            }
        }

        public bool IsFocusTrackingSuspended
        {
            get { return suspendFocusCount > 0; }
        }

        void HookEventHandler(object sender, HookEventArgs e)
        {
            var msg = (WinMsg)Marshal.ReadInt32(e.LParam, IntPtr.Size * 3);

            if (msg == WinMsg.WM_KILLFOCUS)
            {
                IntPtr wParam = Marshal.ReadIntPtr(e.LParam, IntPtr.Size * 2);
                var grid = GetGridFromHandle(wParam);
                if (grid == null)
                    RefreshActiveWindow();
            }
            else if (msg == WinMsg.WM_SETFOCUS)
                RefreshActiveWindow();
        }

        DockGrid GetGridFromHandle(IntPtr hWnd)
        {
            var control = Control.FromChildHandle(hWnd);
            while (control != null)
            {
                var content = control as IDockContent;
                if (content != null)
                {
                    content.Handler.ActiveWindowHandle = hWnd;
                    if (content.Handler.DockPanel == dockPanel)
                        return content.Handler.DockGrid;
                }
                var grid = control as DockGrid;
                if (grid != null && grid.DockPanel == dockPanel)
                    return grid;
                control = control.Parent;
            }
            return null;
        }

        void RefreshActiveWindow()
        {
            SuspendFocus();
            inRefreshActiveWindow = true;

            var oldActiveGrid = activeGrid;
            var oldActiveContent = activeContent;
            var oldActiveDocument = activeDocument;

            SetActiveGrid();
            SetActiveContent();
            SetActiveDocumentGrid();
            SetActiveDocument();
            dockPanel.AutoHideControl.RefreshActiveGrid();

            ResumeFocus();
            inRefreshActiveWindow = false;

            if (oldActiveGrid != activeGrid)
                dockPanel.RaiseActiveGridChanged(EventArgs.Empty);
            if (oldActiveContent != activeContent)
                dockPanel.RaiseActiveContentChanged(EventArgs.Empty);
            if (oldActiveDocument != ActiveDocument)
                dockPanel.RaiseActiveDocumentChanged(EventArgs.Empty);
        }

        DockGrid activeGrid = null;
        public DockGrid ActiveGrid
        {
            get { return activeGrid; }
        }

        void SetActiveGrid()
        {
            var value = GetGridFromHandle(User32.GetFocus());
            if (activeGrid == value)
                return;

            if (activeGrid != null)
                activeGrid.IsActivated = false;

            activeGrid = value;

            if (activeGrid != null)
                activeGrid.IsActivated = true;
        }

        private IDockContent activeContent = null;
        public IDockContent ActiveContent
        {
            get { return activeContent; }
        }

        internal void SetActiveContent()
        {
            IDockContent value = activeGrid == null ? null : activeGrid.ActiveContent;

            if (activeContent == value)
                return;

            if (activeContent != null)
                activeContent.Handler.IsActivated = false;

            activeContent = value;

            if (activeContent != null)
            {
                activeContent.Handler.IsActivated = true;
                if (!activeContent.Handler.IsAutoHide)
                    AddLastToActiveList(activeContent);
            }
        }
        private DockGrid activeDocumentGrid = null;
        public DockGrid ActiveDocumentGrid
        {
            get { return activeDocumentGrid; }
        }
        void SetActiveDocumentGrid()
        {
            DockGrid value = null;

            if (activeGrid != null && activeGrid.DockStyle == DockStyle.Fill)
                value = activeGrid;

            if (value == null && dockPanel.DockWindows != null)
            {
                if (activeDocumentGrid == null)
                    value = dockPanel.DockWindows[DockStyle.Fill].DefaultGrid;
                else if (activeDocumentGrid.DockPanel != dockPanel || activeDocumentGrid.DockStyle == DockStyle.Fill)
                    value = dockPanel.DockWindows[DockStyle.Fill].DefaultGrid;
                else
                    value = activeDocumentGrid;
            }

            if (activeDocumentGrid == value)
                return;

            if (activeDocumentGrid != null)
                activeDocumentGrid.IsActiveDocumentGrid = false;

            activeDocumentGrid = value;

            if (activeDocumentGrid != null)
                activeDocumentGrid.IsActiveDocumentGrid = true;
        }

        IDockContent activeDocument = null;
        public IDockContent ActiveDocument
        {
            get { return activeDocument; }
        }
        void SetActiveDocument()
        {
            activeDocument = activeDocumentGrid == null ? null : activeDocumentGrid.ActiveContent;
        }
    }
}
