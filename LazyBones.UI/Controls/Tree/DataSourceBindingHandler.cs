using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    class DataSourceBindingHandler
    {
        BindingContext bindingContext;
        public DataSourceBindingHandler(BindingContext bindingContext)
        {
            this.bindingContext = bindingContext;
        }
        object dataSource;
        public object DataSource
        {
            get { return dataSource; }
            set
            {
                if (value != null && !(value is IList || value is IListSource))
                    throw new ArgumentException();
                if (dataSource == value)
                    return;
                //SetDataConnection(value, keyMember);
            }
        }
        CurrencyManager dataManager;
        CurrencyManager DataManager
        {
            get { return dataManager; }
            set
            {
                if (dataManager == value)
                    return;
                if (dataManager != null)
                {
                    dataManager.ItemChanged -= new ItemChangedEventHandler(DataManager_ItemChanged);
                    dataManager.PositionChanged -= new EventHandler(DataManager_PositionChanged);
                }

                dataManager = value;

                if (dataManager != null)
                {
                    dataManager.ItemChanged += new ItemChangedEventHandler(DataManager_ItemChanged);
                    dataManager.PositionChanged += new EventHandler(DataManager_PositionChanged);
                }
            }
        }
        void DataManager_PositionChanged(object sender, EventArgs e)
        {
            if (this.dataManager != null)
            {
                //if (AllowSelection)
                //{
                //    this.SelectedIndex = dataManager.Position;
                //}
            }
        }
        void DataManager_ItemChanged(object sender, ItemChangedEventArgs e)
        {
            ItemChanged(e.Index);
        }
        void ItemChanged(int index)
        {
            if (dataManager != null)
            {
                if (index == -1)
                {
                    //SetItemsCore(dataManager.List);
                    if (AllowSelection)
                    {
                        this.SelectedIndex = this.dataManager.Position;
                    }
                }
                else
                {
                   // SetItemCore(e.Index, dataManager[e.Index]);
                }
            }
        }
        public int SelectedIndex { get; set; }
        public bool AllowSelection { get; set; }
        

        
        bool dataConnectionInSetting;
        void SetDataConnection(object newDataSource, BindingMemberInfo newDisplayMember)
        {
            bool dataSourceChanged = dataSource != newDataSource;
            //bool keyMemberChanged = keyMember != newDisplayMember;

            //if (dataConnectionInSetting)
            //{
            //    return;
            //}
            //try
            //{
            //    if (dataSourceChanged || keyMemberChanged)
            //    {
            //        dataConnectionInSetting = true;
            //        var currentList = this.dataManager != null ? this.dataManager.List : null;

            //        UnwireDataSource();

            //        dataSource = newDataSource;

            //        WireDataSource();

            //        if (isDataSourceInitialized)
            //        {

            //            CurrencyManager newDataManager = null;
            //            if (newDataSource != null && bindingContext != null && !(newDataSource == Convert.DBNull))
            //            {
            //                newDataManager = (CurrencyManager)bindingContext[newDataSource, newDisplayMember.BindingPath];
            //            }

            //            DataManager = newDataManager;

            //            //if (DataManager != null && string.IsNullOrEmpty(keyMember.BindingMember))
            //            //{
            //            //    //if (!BindingMemberInfoInDataManager(displayMember))
            //            //    //throw new ArgumentException(SR.GetString(SR.ListControlWrongDisplayMember), "newDisplayMember");
            //            //}

            //            if (DataManager != null)
            //            {
            //                if (keyMemberChanged || (currentList != this.dataManager.List))
            //                {
            //                    ItemChanged(-1);
            //                }
            //            }
            //        }
            //        //this.displayMemberConverter = null;
            //    }

            //    if (dataSourceChanged)
            //    {
            //        //OnDataSourceChanged(EventArgs.Empty);
            //    }

            //    if (keyMemberChanged)
            //    {
            //        //OnDisplayMemberChanged(EventArgs.Empty);
            //    }
            //}
            //finally
            //{
            //    dataConnectionInSetting = false;
            //}
        }
        bool isDataSourceInitialized;
        void UnwireDataSource()
        {
            if (this.dataSource is IComponent)
            {
                ((IComponent)this.dataSource).Disposed -= DataSourceDisposed;
            }
            var dsInit = (this.dataSource as ISupportInitializeNotification);
            if (dsInit != null)
            {
                dsInit.Initialized -= DataSourceInitialized;
            }
        }
        void WireDataSource()
        {
            if (this.dataSource is IComponent)
            {
                ((IComponent)this.dataSource).Disposed += DataSourceDisposed;
            }
            var dsInit = (this.dataSource as ISupportInitializeNotification);
            isDataSourceInitialized = (dsInit == null || dsInit.IsInitialized);
            if (!isDataSourceInitialized)
            {
                dsInit.Initialized += DataSourceInitialized;
            }
        }
        void DataSourceDisposed(object sender, EventArgs e)
        {
            SetDataConnection(null, new BindingMemberInfo(""));
        }

        void DataSourceInitialized(object sender, EventArgs e)
        {
            var dsInit = (this.dataSource as ISupportInitializeNotification);
            isDataSourceInitialized = true;
            //SetDataConnection(this.dataSource, this.keyMember);
        }
    }
}
