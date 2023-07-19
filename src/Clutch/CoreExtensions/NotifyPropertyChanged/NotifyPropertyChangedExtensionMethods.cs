using Clutch.Helpers;

namespace Clutch.CoreExtensions.NotifyPropertyChanged
{
    public static class NotifyPropertyChangedExtensionMethods
    {
        public static T UseNotifyPropertyChanged<T>(this T contextBuilder, NotifyPropertyChangedBehavior behavior) where T : IContextApi =>
            contextBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedExtensionBehavior, behavior, () => new NotifyPropertyChangedExtension());

        public static IAnyEntityTypePropertyBuilder EnableNotifyPropertyChanged(this IAnyEntityTypePropertyBuilder propertyBuilder, bool enable) =>
            propertyBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedEnabled, enable, () => new NotifyPropertyChangedExtension());

        public static IAnyOwnedTypePropertyBuilder EnableNotifyPropertyChanged(this IAnyOwnedTypePropertyBuilder propertyBuilder, bool enable) =>
            propertyBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedEnabled, enable, () => new NotifyPropertyChangedExtension());

        public static IEntitySimplePropertyBuilder<T> EnableNotifyPropertyChanged<T>(this IEntitySimplePropertyBuilder<T> propertyBuilder, bool enable) =>
            propertyBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedEnabled, enable, () => new NotifyPropertyChangedExtension());

        public static IEntityCollectionPropertyBuilder<T, TElement> EnableNotifyPropertyChanged<T, TElement>(this IEntityCollectionPropertyBuilder<T, TElement> propertyBuilder, bool enable) =>
            propertyBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedEnabled, enable, () => new NotifyPropertyChangedExtension());

        public static IOwnedTypeSimplePropertyBuilder<T> EnableNotifyPropertyChanged<T>(this IOwnedTypeSimplePropertyBuilder<T> propertyBuilder, bool enable) =>
            propertyBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedEnabled, enable, () => new NotifyPropertyChangedExtension());

        public static IOwnedTypeCollectionPropertyBuilder<T, TElement> EnableNotifyPropertyChanged<T, TElement>(this IOwnedTypeCollectionPropertyBuilder<T, TElement> propertyBuilder, bool enable) =>
            propertyBuilder.SetExtensionOption(NotifyPropertyChangedExtension.NotifyPropertyChangedEnabled, enable, () => new NotifyPropertyChangedExtension());
    }
}
