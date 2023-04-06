import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { GradesComponent } from './grades.component';
import { GradesSubjectItemComponent } from './grades-subject-item/grades-subject-item.component';
import { GradeItemComponent } from './grade-item/grade-item.component';


const routes: Routes = [
  { path: '', component: GradesComponent }
];

@NgModule({
  declarations: [
    GradesComponent,
    GradesSubjectItemComponent,
    GradeItemComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes)
  ]
})
export class GradesModule { }
