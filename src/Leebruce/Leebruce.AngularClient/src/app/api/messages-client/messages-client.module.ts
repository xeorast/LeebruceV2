import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessagesClientService } from './messages-client.service';



@NgModule( {
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [
    MessagesClientService
  ]
} )
export class MessagesClientModule { }
