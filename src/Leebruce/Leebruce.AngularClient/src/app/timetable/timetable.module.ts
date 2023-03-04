import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { TimetableComponent } from './timetable.component';
import { TimetableClientModule } from '../api/timetable-client/timetable-client.module';
import { TimetableItemComponent } from './timetable-item/timetable-item.component';


const routes: Routes = [
  { path: '', component: TimetableComponent }
];

@NgModule( {
  declarations: [
    TimetableComponent,
    TimetableItemComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild( routes ),
    TimetableClientModule
  ]
} )
export class TimetableModule { }
