﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PictureSorter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PictureSorter.Properties.strings", typeof(strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        internal static string ErrorStr {
            get {
                return ResourceManager.GetString("ErrorStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1. Go to File -&gt; Open to select a picture in the folder containing the pictures.\n\n2. Sort the pictures using Up and Down keys and the Space bar.\n\n3. Go to File -&gt; Export pictures. A new folder is made..
        /// </summary>
        internal static string HelpStr {
            get {
                return ResourceManager.GetString("HelpStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No selected picture found.
        /// </summary>
        internal static string NoPictureCheckedErrorStr {
            get {
                return ResourceManager.GetString("NoPictureCheckedErrorStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select the folder containing the images.
        /// </summary>
        internal static string SelectFolderStr {
            get {
                return ResourceManager.GetString("SelectFolderStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select a picture.
        /// </summary>
        internal static string SelectPictureStr {
            get {
                return ResourceManager.GetString("SelectPictureStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to sort.
        /// </summary>
        internal static string sortStr {
            get {
                return ResourceManager.GetString("sortStr", resourceCulture);
            }
        }
    }
}
