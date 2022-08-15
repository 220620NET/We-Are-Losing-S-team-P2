import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Component, OnDestroy, OnInit, EventEmitter, Output} from '@angular/core';
import { ComsinisApiServiceService } from '../services/Comsini-api-service/comsinis-api-service.service';
import { Cosminis } from '../Models/Cosminis';

@Component({
  selector: 'app-all-cosminis',
  templateUrl: './all-cosminis.component.html',
  styleUrls: ['./all-cosminis.component.css']
})
export class AllCosminisComponent implements OnInit {

  constructor(private api:ComsinisApiServiceService) { }
  showCosminis!:Promise<boolean>;
  cosminis : Cosminis[] = [];

  cosminis1 : Cosminis = 
  {
    companionId : 1,
    trainerId : 1,
    userFk : 1,
    speciesFk : 1,
    nickname : "Shrek",
    emotion : 100,
    hunger : 100
  }

  updateCosminis() : void {
      this.api.getAllComsinis().subscribe((res) => 
      {
        console.log(res);
        // this.cosminis = res;
        console.log(this.cosminis);
        this.showCosminis=Promise.resolve(true);
      })
  }

  getCosminiByID(ID : number) : void {
    this.api.getCosminiByID(ID).subscribe((res) => 
    {
      console.log(res);
      this.cosminis1 = res;
      console.log(this.cosminis1);
    })
}

showCards = false;
  ngOnInit(): void 
  {
    // this.showCards = false;
    this.api.getAllComsinis().subscribe((res) => 
    {
      console.log(res);
      this.cosminis = res;
      this.showCards = true;
      console.log(this.cosminis);
      this.showCosminis=Promise.resolve(true);
    })
  }

}
