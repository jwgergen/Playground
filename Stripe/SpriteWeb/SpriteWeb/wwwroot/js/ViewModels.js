// customer
var CustomerViewModel = function (data) {
    var self = this;

    self.construct = function (data) {
        $.extend(true, self, {
            selected: ko.observable(false),
            charges: ko.observableArray(),
            selectedCharge: ko.observable(),
            refunding: ko.observable(false),
            creating: ko.observable(false),
            execAmount: ko.observable(),

            // computed
            createRefundButtonLabel: ko.pureComputed(function () {
                if (self.refunding()) {
                    return "Refund!";
                } else {
                    return "Create!";
                }
            }),
        });

        ko.mapping.fromJS(data, {}, self);
    }

    if (data) {
        self.construct(data);
    }


    self.select = function (bSelected) {
        self.selected(bSelected);
        self.selectedCharge(null);
        self.refunding(false);
        self.creating(false);
        if (bSelected) {
            // load the charges for this customer
            self.loadCharges();
        } else {
            self.unloadCharges();
        }
        return self;
    };

    self.create = function (name, email, callback) {
        var params = {
            Name: name,
            Email: email,
        };

        return $.post('api/Customers', params)
            .done(function (data) {
                if (data) {
                    self.construct(data);
                    if ($.isFunction(callback)) {
                        callback(self);
                    }
                }
            }).fail(function (data) {
                alert(data.statusText);
            });
    }

    self.delete = function (callback) {

        var params = {
            id: self.id(),
        };

        return $.ajax({
            url: 'api/customers/' + self.id(),
            type: 'DELETE'
        })
            .done(function () {
                if ($.isFunction(callback)) {
                    callback(self);
                }
            })
    }

    self.loadCharges = function () {
        var params = {
            id: self.id(),
        };

        //        return $.get('api/stripe/getcharges', params)
        return $.get('api/charges/getbycustomer', params)
            .done(function (data) {
                debugger;
                if (data) {
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            return new ChargeViewModel(options.data);
                        },
                    }, self.charges);
                }
            })
            .fail(function (data) {
                alert(data.statusText);
            });
    };

    self.unloadCharges = function () {
        self.charges.removeAll();
    };


    // event handlers...
    self.refundClicked = function () {
        self.refunding(true);
        self.creating(false);
        var val = Math.abs(self.selectedCharge().amountCaptured() - self.selectedCharge().amountRefunded());
        self.execAmount(val);
    };

    self.createClicked = function () {
        self.creating(true);
        self.refunding(false);
        self.execAmount(null);
    };

    self.selectCharge = function (charge) {
        self.selectedCharge(null);
        $.each(self.charges(), function (i, o) {
            o.select(false);
        });
        charge.select(true);
        self.selectedCharge(charge);
    };
    // create a charge ||
    // create a refund
    self.executeCreateRefund = function () {
        try {
            if (self.refunding()) {
                self.selectedCharge().refund(self.execAmount(), function () {
                    $.get('api/charges/getbyid', { id: self.selectedCharge().id })
                        .done(function (charge) {
                            self.charges.replace(
                                self.selectedCharge(),
                                new ChargeViewModel(charge));
                        })
                        .fail(function (data) {
                            alert(data.statusText);
                        });
                });
            } else {
                // create a new charge
                var charge = new ChargeViewModel().create(self.id(),
                    self.execAmount(), function (newCharge) {
                        self.charges.unshift(newCharge);
                    });
            }
        }
        finally {
            self.refunding(false);
            self.creating(false);
        }
    };

};


// charges
var ChargeViewModel = function (data) {
    var self = this;

    self.construct = function (data) {
        $.extend(true, self, {
            selected: ko.observable(false),
            netAmount: ko.pureComputed(function () {
                var amount = self.amount();
                var refund = self.amountRefunded();

                return (amount - refund).toString();
            }),
        });
        ko.mapping.fromJS(data, {}, self);
    }

    if (data) {
        self.construct(data);
    }

    // behaviors
    self.select = function (bSelected) {
        self.selected(bSelected);
        return self;
    };

    self.refund = function (amount, callback) {
        var params = {
            Id: self.id(),
            Amount: amount,
        };

        return $.ajax({
            url: 'api/charges/refund',
            data: { refund: params },
            type: 'PUT',
            'content-type': 'application/json',
        })
            .done(function (data) {
                if ($.isFunction(callback)) {
                    callback();
                }
            })
            .fail(function (data) {
                alert(data.statusText)
            });
    };

    self.create = function (customerId, amount, callback) {
        var params = {
            Id: customerId,
            Amount: amount,
        };
        return $.post('api/charges/create', params)
            .done(function (data) {
                if (data) {
                    self.construct(data);
                    if ($.isFunction(callback)) {
                        callback(self);
                    }
                }
            })
            .fail(function (data) {
                alert(data.statusText);
            });
    };


};

