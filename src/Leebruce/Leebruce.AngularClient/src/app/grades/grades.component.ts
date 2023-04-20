import { Component, OnDestroy, OnInit } from '@angular/core';
import { Modal } from 'bootstrap';
import { GradeModel, GradesClientService, GradesDataModel } from '../api/grades-client/grades-client.service';
import { GradesViewModel, SubjectGradesViewModel } from './grades.view-model';

@Component( {
  selector: 'app-grades',
  templateUrl: './grades.component.html',
  styleUrls: ['./grades.component.scss']
} )
export class GradesComponent implements OnInit, OnDestroy {

  constructor(
    private gradesService: GradesClientService
  ) { }

  public model?: GradesViewModel
  public selected?: SubjectGradesViewModel
  public showOnlyNew: boolean = false

  public modalBody: string = ""
  private modalObj?: Modal

  ngOnInit(): void {
    let allModel: GradesDataModel | undefined = undefined
    let newModel: GradesDataModel | undefined = undefined

    this.gradesService.getGrades().subscribe( res => {
      this.finishFetch( allModel = res, newModel )
    } )
    this.gradesService.getNewGrades().subscribe( res => {

      this.finishFetch( allModel, newModel = res )
    } )

    let modalElem = document.getElementById( 'subjectGradesModal' )!
    this.modalObj = new Modal( modalElem, {} )
  }

  ngOnDestroy(): void {
    this.modalObj?.dispose()
  }

  finishFetch( allModel: GradesDataModel | undefined, newModel: GradesDataModel | undefined ) {
    if ( !allModel || !newModel ) {
      return
    }

    let subjects = GradesComponent.mapResultSubjects( allModel )

    let firstTermNewGrades: { [subject: string]: GradeModel[] } = {}
    let secondTermNewGrades: { [subject: string]: GradeModel[] } = {}
    for ( const subject of newModel.subjects ) {
      firstTermNewGrades[subject.subject] = subject.firstTermGrades
      secondTermNewGrades[subject.subject] = subject.secondTermGrades
    }

    for ( const subject of subjects ) {
      subject.newGrades = {
        firstTerm: firstTermNewGrades[subject.subject],
        secondTerm: secondTermNewGrades[subject.subject]
      }
    }

    this.model = {
      isByPercent: allModel.isByPercent,
      subjects: subjects
    }
  }

  static mapResultSubjects( data: GradesDataModel ) {
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

  toggleShowOnlyNew() {
    this.showOnlyNew = !this.showOnlyNew
  }

  firstTermGrades() {
    if ( !this.selected )
      return undefined
    return this.showOnlyNew
      ? this.selected.newGrades.firstTerm
      : this.selected.firstTermGrades
  }
  secondTermGrades() {
    if ( !this.selected )
      return undefined
    return this.showOnlyNew
      ? this.selected.newGrades.secondTerm
      : this.selected.secondTermGrades
  }

}
