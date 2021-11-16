using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;

#nullable disable

namespace Fricke.Test.Fakes
{
    /// <summary>
    /// A TempData dictionary used to back TempData during a test run.
    /// </summary>
    public class TestTempDataDictionary : Dictionary<string, object>, ITempDataDictionary
    {
        ///<inheritdoc/>
        object IDictionary<string, object>.this[string key]
        {
            get
            {
                if (TryGetValue(key, out object value))
                {
                    return value;
                }

                return null;
            }
            set
            {
                base[key] = value;
            }
        }

        ///<inheritdoc/>
        ///<remarks><b>This method is not currently implemented.</b></remarks>
        public void Keep()
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        ///<remarks><b>This method is not currently implemented.</b></remarks>
        public void Keep(string key)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        ///<remarks><b>This method is not currently implemented.</b></remarks>
        public void Load()
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        ///<remarks><b>This method is not currently implemented.</b></remarks>
        public object Peek(string key)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        ///<remarks><b>This method is not currently implemented.</b></remarks>
        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
