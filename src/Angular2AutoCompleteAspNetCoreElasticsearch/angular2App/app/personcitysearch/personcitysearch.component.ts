import { Injectable, Component, Inject, EventEmitter, Input, Output, OnInit, AfterViewInit, ElementRef } from '@angular/core';
import { Http, Response } from "@angular/http";

import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';
import { Router } from  '@angular/router';

import { Configuration } from '../app.constants';
import { PersoncitysearchService } from './personcitysearchService';
import { PersonCity } from '../model/personCity';

import { CompleterService, CompleterItem } from 'ng2-completer';

@Component({
    selector: 'personcitysearch',
  template: `
<ng2-completer [dataService]="dataService" (selected)="onTermSelected($event)" [minSearchLength]="0" [disableInput]="disableAutocomplete"></ng2-completer>

`,
  styles: [String(require('./personcitysearch.component.scss'))]
})

@Injectable()
export class PersoncitysearchComponent implements OnInit    {

    constructor(private completerService: CompleterService, private http: Http, private _configuration: Configuration) {

        this.dataService = new PersoncitysearchService(http, _configuration);
    }

    @Output() bindSelectedTermChange = new EventEmitter<string>();
    @Input() bindSelectedTerm: string;

    @Input() disableAutocomplete: boolean = false;

    private searchStr: string;
    private dataService: PersoncitysearchService;

    ngOnInit() {
        console.log("ngOnInit SearchComponent");
    }

    public onTermSelected(selected: CompleterItem) {
        console.log(selected);

        this.bindSelectedTermChange.emit(selected.title);
    }
}
