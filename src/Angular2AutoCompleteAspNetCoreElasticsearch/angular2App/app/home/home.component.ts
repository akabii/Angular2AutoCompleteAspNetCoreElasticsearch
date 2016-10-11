import { Observable } from 'rxjs/Observable';
import { Component, OnInit } from '@angular/core';
import { Http } from '@angular/http';

import { AutocompleteSearchComponent } from '../autocomplete/search.component';
import { SearchDataService } from '../autocomplete/searchDataService';

@Component({
    selector: 'homecomponent',
    template: require('./home.component.html'),
    providers: [SearchDataService]
})

export class HomeComponent implements OnInit {

    public message: string;
    public PersonCityItems: any[];

    constructor(private _dataService: SearchDataService) {
        this.message = "Hello from HomeComponent constructor";
    }

    ngOnInit() {
        this._dataService
            .GetAll()
            .subscribe(data => this.PersonCityItems = data,
            error => console.log(error),
            () => console.log('Get all complete'));
    }
}
