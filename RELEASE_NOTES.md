#### 2.5.4
* BUGFIX: Codegen for INPC004 use [CallerMemberName].

#### 2.5.3
* PERF: Avoid calls to SemantiModel and merge analyzers.
* BUGFIX: Codegen for INPC009.
* BUGFIX: Codegen for INPC003.
* FEATURE: New analyzer INPC015 check if property is recursive.
* FEATURE: New analyzer INPC016 check that backing field is assigned before notifying.

#### 2.5.2
* BUGFIX: Handle check in lock INPC005.
* FEATURE: Codefix INPC007, seal class.
* BUGFIX: handle property only assigned in ctor.

#### 2.5.1
* BUGFIX: INPC014 igore when setter has validation.
* BUGFIX: INPC001 ignore attribute

#### 2.5.0
* BUGFIX: Handle backing field named as keyword 
* FEATURE: Support Prism.Mvvm

#### 2.2.1
* BUGFIX: Handle arbitraty check if different.
* FEATURE: Suggest refactor to set and raise method.

#### 2.2.1
* BUGFIX: Handle side effect in if return.
* FEATURE: Check that assignment is before notification.

#### 2.2.0
* FEATURE: Support MvvmCross.

#### 2.1.0
* FEATURE: Support Stylet.
