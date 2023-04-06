import { Component, OnInit } from '@angular/core';
import { Modal } from 'bootstrap';
import { GradeModel, GradesClientService, SubjectGradesModel } from '../api/grades-client/grades-client.service';
import { GradesViewModel as SubjectGradesViewModel } from './grades.view-model';

@Component( {
  selector: 'app-grades',
  templateUrl: './grades.component.html',
  styleUrls: ['./grades.component.scss']
} )
export class GradesComponent implements OnInit {

  constructor(
    private gradesService: GradesClientService
  ) { }

  public subjects?: SubjectGradesViewModel[]
  public selected?: SubjectGradesViewModel
  public modalBody: string = ""

  private modalObj?: Modal

  ngOnInit(): void {
    this.gradesService.getGrades().subscribe( {
      next: res => {
        this.subjects = this.mapResult( res )
      }
    } )

    let modalElem = document.getElementById( 'subjectGradesModal' )!
    this.modalObj = new Modal( modalElem, {} )
  }

  mapResult( subjects: SubjectGradesModel[] ) {
    let subjectsVm = subjects as unknown as SubjectGradesViewModel[]
    for ( const subject of subjectsVm ) {
      let valuedGrades = subject.grades.filter( g => g.value && g.value != 0 && g.weight && g.countToAverage )
      let weightsSum = valuedGrades.reduce( ( sum: number, curr: GradeModel ) => sum + curr.weight!, 0 )
      let gradeSum = valuedGrades.reduce( ( sum: number, curr: GradeModel ) => sum + curr.weight! * curr.value!, 0 )

      subject.average = weightsSum == 0 ? null : gradeSum / weightsSum
      subject.weightsSum = weightsSum
      subject.gradesPart1 = subject.grades.slice( 0, 2 * Math.floor( subject.grades.length / 3 ) )
      subject.gradesPart2 = subject.grades.slice( 2 * Math.floor( subject.grades.length / 3 ), subject.grades.length - 1 )
    }


    return subjectsVm.filter( s => s.average )
  }

  select( subject: SubjectGradesViewModel ) {
    this.selected = subject
    this.modalObj?.show()

    this.modalBody = "" + subject.average
  }

}
