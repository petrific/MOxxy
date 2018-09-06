import { TestBed, inject } from '@angular/core/testing';

import { MockServerService } from './mock-server.service';

describe('MockServerService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MockServerService]
    });
  });

  it('should be created', inject([MockServerService], (service: MockServerService) => {
    expect(service).toBeTruthy();
  }));
});
