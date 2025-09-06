import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { Journey, JourneyCreateRequest, JourneyUpdateRequest, TransportType } from '../../models';

@Component({
  selector: 'app-journey-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './journey-form.component.html',
  styleUrls: ['./journey-form.component.scss']
})
export class JourneyFormComponent implements OnInit, OnDestroy {
  @Input() journey?: Journey;
  @Input() isEditMode = false;
  @Output() save = new EventEmitter<JourneyCreateRequest | JourneyUpdateRequest>();
  @Output() cancel = new EventEmitter<void>();

  journeyForm!: FormGroup;
  transportTypes = Object.values(TransportType);
  isSubmitting = false;
  private destroy$ = new Subject<void>();

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
    if (this.journey && this.isEditMode) {
      this.populateForm();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.journeyForm = this.fb.group({
      startLocation: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      startTime: ['', [Validators.required]],
      arrivalLocation: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      arrivalTime: ['', [Validators.required]],
      transportType: [TransportType.Car, [Validators.required]],
      distanceKm: [0, [Validators.required, Validators.min(0.01), Validators.max(9999.99)]]
    });

    this.journeyForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.validateTimeOrder();
      });
  }

  private populateForm(): void {
    if (this.journey) {
      this.journeyForm.patchValue({
        startLocation: this.journey.startLocation,
        startTime: this.formatDateTimeForInput(this.journey.startTime),
        arrivalLocation: this.journey.arrivalLocation,
        arrivalTime: this.formatDateTimeForInput(this.journey.arrivalTime),
        transportType: this.journey.transportType,
        distanceKm: this.journey.distanceKm
      });
    }
  }

  private formatDateTimeForInput(dateTime: string): string {
    const date = new Date(dateTime);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  private validateTimeOrder(): void {
    const startTime = this.journeyForm.get('startTime')?.value;
    const arrivalTime = this.journeyForm.get('arrivalTime')?.value;

    if (startTime && arrivalTime) {
      const start = new Date(startTime);
      const arrival = new Date(arrivalTime);

      if (arrival <= start) {
        this.journeyForm.get('arrivalTime')?.setErrors({ 
          invalidTimeOrder: 'Arrival time must be after start time' 
        });
      } else {
        this.journeyForm.get('arrivalTime')?.setErrors(null);
      }
    }
  }

  onSubmit(): void {
    if (this.journeyForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      
      const formValue = this.journeyForm.value;
      
      const startTime = new Date(formValue.startTime).toISOString();
      const arrivalTime = new Date(formValue.arrivalTime).toISOString();

      if (this.isEditMode && this.journey) {
        const updateData: JourneyUpdateRequest = {
          startLocation: formValue.startLocation,
          startTime,
          arrivalLocation: formValue.arrivalLocation,
          arrivalTime,
          transportType: formValue.transportType,
          distanceKm: formValue.distanceKm
        };
        this.save.emit(updateData);
      } else {
        const createData: JourneyCreateRequest = {
          startLocation: formValue.startLocation,
          startTime,
          arrivalLocation: formValue.arrivalLocation,
          arrivalTime,
          transportType: formValue.transportType,
          distanceKm: formValue.distanceKm,
          isFavorite: false
        };
        this.save.emit(createData);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.journeyForm.controls).forEach(key => {
      const control = this.journeyForm.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(controlName: string): string {
    const control = this.journeyForm.get(controlName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `${this.getFieldDisplayName(controlName)} is required`;
      }
      if (control.errors['minlength']) {
        return `${this.getFieldDisplayName(controlName)} must be at least ${control.errors['minlength'].requiredLength} characters`;
      }
      if (control.errors['maxlength']) {
        return `${this.getFieldDisplayName(controlName)} must be no more than ${control.errors['maxlength'].requiredLength} characters`;
      }
      if (control.errors['min']) {
        return `${this.getFieldDisplayName(controlName)} must be at least ${control.errors['min'].min}`;
      }
      if (control.errors['max']) {
        return `${this.getFieldDisplayName(controlName)} must be no more than ${control.errors['max'].max}`;
      }
      if (control.errors['invalidTimeOrder']) {
        return control.errors['invalidTimeOrder'];
      }
    }
    return '';
  }

  private getFieldDisplayName(controlName: string): string {
    const displayNames: { [key: string]: string } = {
      startLocation: 'Start location',
      startTime: 'Start time',
      arrivalLocation: 'Arrival location',
      arrivalTime: 'Arrival time',
      transportType: 'Transport type',
      distanceKm: 'Distance'
    };
    return displayNames[controlName] || controlName;
  }

  isFieldInvalid(controlName: string): boolean {
    const control = this.journeyForm.get(controlName);
    return !!(control?.invalid && control?.touched);
  }
} 