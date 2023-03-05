import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { MessagesComponent } from './messages.component';


const routes: Routes = [
  { path: '', component: MessagesComponent }
];

@NgModule({
  declarations: [
    MessagesComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes)
  ]
})
export class MessagesModule { }
