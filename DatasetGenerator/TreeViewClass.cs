using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Storage;

namespace DatasetGenerator
{
    class TreeViewClass : DependencyObject
    {
        public IStorageItem StorageItem { get; set; }


        public bool IsFolder
        {
            get { return (bool)GetValue(IsFolderProperty); }
            set { SetValue(IsFolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFolderProperty =
            DependencyProperty.Register("IsFolder", typeof(bool), typeof(TreeViewClass), new PropertyMetadata(false));



        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(TreeViewClass), new PropertyMetadata("Name"));




    }
}
