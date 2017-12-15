import { Component, Inject, OnInit } from '@angular/core';
import { Http } from '@angular/http';
import { Router } from '@angular/router';

@Component({
    selector: 'nav-menu',
    templateUrl: './navmenu.component.html',
    styleUrls: ['./navmenu.component.css']
})
    

export class NavMenuComponent implements OnInit{
    public userName = "";
    private mHttp: Http;
    private mBaseUrl: string;
    constructor(http: Http, router: Router, @Inject('BASE_URL') baseUrl: string) {       
        this.mHttp = http;
        this.mBaseUrl = baseUrl;

        http.get(baseUrl + 'api/Job/GetUserName').subscribe(result => {
            this.userName = result.text();           
        }, error => console.error(error));
    }    
    ngOnInit() {
        this.mHttp.get(this.mBaseUrl + 'api/Job/GetUserName').subscribe(result => {
            this.userName = result.text();
        }, error => console.error(error));
    }
}
