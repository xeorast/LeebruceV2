import { Component, OnInit } from '@angular/core';
import { Modal } from 'bootstrap';
import { GradeModel, GradesClientService, GradesDataModel } from '../api/grades-client/grades-client.service';
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

  mapResult( data: GradesDataModel ) {
    let subjectsVm = data.subjects as unknown as SubjectGradesViewModel[]
    for ( const subject of subjectsVm ) {
      if ( !data.isByPercent ) {
        let valuedGrades = subject.firstTermGrades.filter( g => g.value && g.value != 0 && g.weight && g.countToAverage )
          .concat( subject.secondTermGrades.filter( g => g.value && g.value != 0 && g.weight && g.countToAverage ) )

        subject.weightsSum = valuedGrades.reduce( ( sum: number, curr: GradeModel ) => sum + curr.weight!, 0 )
        subject.percent = subject.average ? ( subject.average - 1 ) * 100 / 5 : undefined
      }
    }

    return subjectsVm.filter( s => s.percent )
  }

  select( subject: SubjectGradesViewModel ) {
    this.selected = subject
    this.modalObj?.show()

    this.modalBody = "" + subject.average
  }

}
