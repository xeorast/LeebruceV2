import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { GradesClientService } from './grades-client.service';



@NgModule( {
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [
    GradesClientService
  ]
} )
export class GradesClientModule { }
