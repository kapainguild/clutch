using System.Collections.Generic;

namespace Clutch.Building
{
    public class ProxyInfluencers
    {
        private readonly List<ITypeInfluencer> _typeInfluencers = new List<ITypeInfluencer>();

        private readonly List<IPropertySetterInfluencer> _propertySetterInfluencers = new List<IPropertySetterInfluencer>();


        public void AddTypeInfluencer(ITypeInfluencer influencer)
        {
            _typeInfluencers.Add(influencer);
        }

        public void AddPropertySetterInfluencer(IPropertySetterInfluencer influencer)
        {
            _propertySetterInfluencers.Add(influencer);
        }

        public ITypeInfluencer[] GetTypeInfluencers()
        {
            return _typeInfluencers.ToArray();
        }

        public IPropertySetterInfluencer[] GetPropertySetterInfluencers()
        {
            return _propertySetterInfluencers.ToArray();
        }
    }
}
