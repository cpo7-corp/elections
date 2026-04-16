import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './contact.html',
  styleUrl: './contact.less'
})
export class ContactComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);
  loading = false;

  contactForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phone: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    message: ['', Validators.required]
  });

  onSubmit() {
    if (this.contactForm.valid) {
      // הקריאה מתבצעת ישירות מהקומפוננטה כפי שביקשת
      this.loading = true;
      this.http.post(`${environment.baseApiUrl}/contact/contact`, this.contactForm.value)
        .pipe(finalize(() => this.loading = false))
        .subscribe({
          next: (response: any) => {
            console.log('Success:', response);
            if (response.success) {
              alert('Message sent successfully!');
              this.contactForm.reset();
            } else {
              alert(response.error || 'Failed to send message.');
            }
          },
          error: (error) => {
            console.error('Error:', error);
            const errorMessage = error.error?.error || 'Failed to send message. Please try again.';
            alert(errorMessage);
          }
        });
    }
  }
}
