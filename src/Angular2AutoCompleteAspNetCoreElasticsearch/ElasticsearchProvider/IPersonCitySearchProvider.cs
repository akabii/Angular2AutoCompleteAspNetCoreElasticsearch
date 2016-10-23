﻿using System.Collections.Generic;

namespace SearchComponent
{
    public interface IPersonCitySearchProvider
    {
        void CreateIndex();

        void CreateTestData();

        IEnumerable<string> AutocompleteSearch(string term);

        IEnumerable<PersonCity> Search(string term);

        bool GetStatus();

        IEnumerable<PersonCity> QueryString(string term);
    }
}