// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
dollarAmount = function (val) {
    var dollarAmount = numeral(val / 100);
    return dollarAmount.format('$0,0.00');
};

chargeDate = function (val) {
    var d = new Date(val);
    return formatDate(d);
}

formatDate = function (d) {
    var month = d.getMonth() + 1;
    var day = d.getDate();
    var year = d.getFullYear();

    var dateArray = [month, day, year];

    var dateString = dateArray.join('/');

    return dateString;
}

// View model code goes here
var ViewModel = function () {
    var self = this;

    $.extend(true, self, {
        requestResult: ko.observable("something"),
        customers: ko.observableArray(),
        selectedCustomer: ko.observable(),
        customerLimit: ko.observable(20),
        // new customers...
        creatingCustomer: ko.observable(false),
        customerName: ko.observable('Jim Negreg'),
        customerEmail: ko.observable('jim@something.com'),

        // paid charges
        fromDate: ko.observable(),
        paidCharges: ko.observableArray(),
        limit: ko.observable(20),
    });

    searchDateChanged = function () {
        var self = this;
        if (self.fromDate()) {
            // query the api for the payments.
            self.getPayments();
        }
    };

    self.getPayments = function () {
        var params = {
            FromDate: self.fromDate(),
            Limit: self.limit(),
        };

        $.get('api/charges/getpayments', params)
            .done(function (data) {
                if (data) {
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            return new ChargeViewModel(options.data);
                        },
                    }, self.paidCharges);
                }
            })
            .fail(function (data) {
                alert(data.statusText);
            });

    }

    self.createCustomerClicked = function () {
        self.creatingCustomer(true);
    }

    self.createCustomer = function () {
        // create the customer here.
        try {
            var customer = new CustomerViewModel().create(
                self.customerName(),
                self.customerEmail(), function (cust) {
                    self.customers.unshift(cust);
                });
        } finally {
            self.creatingCustomer(false);
        }
    }

    self.executeDeleteCustomer = function () {
        if (self.selectedCustomer()) {
            self.selectedCustomer().delete()
                .done(function () {
                    self.customers.remove(self.selectedCustomer());
                    self.selectedCustomer(null);
                })
                .fail(function (data) {
                    debugger;
                    alert(data.statusText);
                });
        }
    }



    self.customerClicked = function (item) {
        self.selectedCustomer(null);

        // clear all the selections
        $.each(self.customers(), function (i, o) {
            o.select(false);
        });

        // select this item..
        self.selectedCustomer(item.select(true));
    };

    self.chargeClicked = function (item) {
        self.selectedCharge(null);
        $.each(self.charges(), function (i, o) {
            o.select(false);
        });
        item.select(true);
        self.selectedCharge(item);
    }

    self.getCustomers = function () {

        if (self.selectedCustomer()) {
            self.selectedCustomer().select(false);
            self.selectedCustomer(null);
        }
        // get the customers...
        self.customers(null);

        params = {
            limit: self.customerLimit(),
        };
        $.get('api/Customers/' + self.customerLimit())
            .done(function (data) {
                if (data) {
                    ko.mapping.fromJS(data, {
                        create: function (options) {
                            return new CustomerViewModel(options.data);
                        },
                    }, self.customers);
                }
            })
            .fail(function (data) {
                debugger;
            });
    }

    self.init = function () {
        self.getCustomers();
    };
};

