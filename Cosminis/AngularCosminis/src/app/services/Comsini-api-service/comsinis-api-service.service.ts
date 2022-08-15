import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Cosminis } from '../../Models/Cosminis';

@Injectable({
  providedIn: 'root'
})
export class ComsinisApiServiceService {

  constructor(private http: HttpClient) { }
  url : string = environment.api;

  getAllComsinis() : Observable<Cosminis[]> {
    return this.http.get(this.url + `companions/GetAll`) as Observable<Cosminis[]>;
  }

  getCosminiByID(ID : number) : Observable<Cosminis> {
    return this.http.get(this.url + `companions/SearchByCompanionId?companionId=${ID}`) as Observable<Cosminis>;
  }
}
