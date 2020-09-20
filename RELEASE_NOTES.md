#### 3.2.1:
* Handle regressions in Roslyn 3.7

#### 3.2.0:
 * INPC0023 Instance equals for reference type
 * INPC024 ReferenceEquals for value type

#### 3.1.1
* Solution wide nullable fixes.
* INPC002 ignores when [Bindable(false)]

#### 3.1.0
* FEATURE: Handle nullable.
* FEATURE: Fixes for nullable warnings.

#### 3.0.0
* BREAKING: VS2019+

#### 2.7.2
* BUGFIX: Figure out caching in ConcurrentDictionary

#### 2.7.1
* BUGFIX INPC002 when event accessors.
* BUGFIX INPC010 when side effects in setter.
* BUGFIX INPC002 when setting other property that notifies.

#### 2.7.0
* INPC021 Setter should set backing field.
* INPC020 Prefer expression body accessor.
* INPC019 Getter should return backing field.
* INPC018 PropertyChanged invoker should be protected when class is not sealed.

#### 2.6.0
* FEATURE: Support new MvvmCross.
* FEATURE: Recognize [NotifyPropertyChangedInvocatorAttribute]

#### 2.5.12
* BUGFIXES: INPC017.

#### 2.5.8
* FEATURE: Re-enabled detecting OnPropertyChanged and TrySet from outside the compilation.

#### 2.5.6
* BUGFIX: Handle nested fields and properties.

#### 2.5.5
* FEATURE: Alternative codefixes that adds usings.

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
* BUGFIX: Handle arbitrary check if different.
* FEATURE: Suggest refactor to set and raise method.

#### 2.2.1
* BUGFIX: Handle side effect in if return.
* FEATURE: Check that assignment is before notification.

#### 2.2.0
* FEATURE: Support MvvmCross.

#### 2.1.0
* FEATURE: Support Stylet.
