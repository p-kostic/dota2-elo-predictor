# INFOB3OMG
We took the "[INFOB3OMG] Research methods for game technology" course at Utrecht University. Our research question was whether the Elo rating system could be used to predict the outcome of the popular multiplayer game Dota 2. The final report can be found in the repository above. This GitHub Readme.md is essentially the Supplementary Online Material and it contains additional statistical analysis that weren't included in the final report.

The report contains some [controversial](http://stats.stackexchange.com/questions/3559/which-pseudo-r2-measure-is-the-one-to-report-for-logistic-regression-cox-s) values, in particular pseudo-R^2 and p-values. Since a presentation will be given to students that might have  no background in statistics, these values made it to our report.


### Statistical Analysis
This section will be updated with explanations very soon.
#### Coefficients and exponentiated cofficients

``` R
> summary(model)
Call:
glm(formula = outcome ~ elo.diff, family = binomial, data = train, 
    na.action = na.omit)

Deviance Residuals: 
    Min       1Q   Median       3Q      Max  
-1.8795  -1.1932   0.8441   1.1213   2.1269  

Coefficients:
             Estimate Std. Error z value Pr(>|z|)    
(Intercept) 0.1179609  0.0146489   8.053 8.11e-16 ***
elo.diff    0.0040826  0.0001556  26.232  < 2e-16 ***
---
Signif. codes:  0 ‘***’ 0.001 ‘**’ 0.01 ‘*’ 0.05 ‘.’ 0.1 ‘ ’ 1

(Dispersion parameter for binomial family taken to be 1)

    Null deviance: 26953  on 19476  degrees of freedom
Residual deviance: 26187  on 19475  degrees of freedom
AIC: 26191

Number of Fisher Scoring iterations: 4

> confint(model)               # CI
                  2.5 %      97.5 %
(Intercept) 0.089259379 0.146683182
elo.diff    0.003778959 0.004389055

> exp(model$coefficients)        # exponentiated coefficients
(Intercept)    elo.diff 
   1.125200    1.004091 
   
> exp(confint(model))            # 95% CI for exponentiated coefficients
               2.5 %   97.5 %
(Intercept) 1.093364 1.157987
elo.diff    1.003786 1.004399
```

#### Analysis of variance for individual terms
``` R
> library(car)
> Anova(model, type="II", test="Wald")
Analysis of Deviance Table (Type II tests)

Response: outcome
         Df  Chisq Pr(>Chisq)    
elo.diff  1 688.14  < 2.2e-16 ***
---
Signif. codes:  0 ‘***’ 0.001 ‘**’ 0.01 ‘*’ 0.05 ‘.’ 0.1 ‘ ’ 1

```

#### Pseudo R-squared
``` R
> library(rcompanion)
> nagelkerke(model)
$Models
                                                          
Model: "glm, outcome ~ elo.diff, binomial, train, na.omit"
Null:  "glm, outcome ~ 1, binomial, train, na.omit"       

$Pseudo.R.squared.for.model.vs.null
                             Pseudo.R.squared
McFadden                            0.0284187
Cox and Snell (ML)                  0.0385636
Nagelkerke (Cragg and Uhler)        0.0514603

$Likelihood.ratio.test
 Df.diff LogLik.diff  Chisq    p.value
      -1     -382.98 765.97 1.353e-168 
$Messages
[1] "Note: For models fit with REML, these statistics are based on refitting with ML"
```
#### Overall p-value test
``` R
> anova(model, 
+       update(model, ~1),    # update here produces null model for comparison 
+       test="Chisq")
Analysis of Deviance Table

Model 1: outcome ~ elo.diff
Model 2: outcome ~ 1
  Resid. Df Resid. Dev Df Deviance  Pr(>Chi)    
1     19475      26187                          
2     19476      26953 -1  -765.97 < 2.2e-16 ***
---
Signif. codes:  0 ‘***’ 0.001 ‘**’ 0.01 ‘*’ 0.05 ‘.’ 0.1 ‘ ’ 1
```
#### Plot of standardized residuals
``` R
plot(fitted(model), rstandard(model))
```
![plot](http://i.imgur.com/9rLWvyf.png)

## Future Work
### Raw data
Magnet link to raw match data in tar -zcvf (gzip) format, including a sample of what the data looks like.
If only `x` of matches is needed, use `zcat rawMatches.tar.gz | head -x > newfile.json` to decompress the first `x` number of matches 
```
magnet:?xt=urn:btih:3e862a0f2073ae76a66c4f054c2fe6c45c80ea19&dn=raw+match+data
```
If nobody is seeding, contact me: snookik@gmail.com

### Statistic Procedures in R (that were used for the report).
#### Reading and splitting the data into train and test set
``` R
#read the file
mydata = read.csv("path", sep = ";")

# split the index to 80% of the sample size
smp_size <- floor(0.80 * nrow(mydata))

# set the seed for the shuffle to make our partition reproductible
set.seed(123)
train_ind <- sample(seq_len(nrow(mydata)), size = smp_size)

# split the data using the index
train <- mydata[train_ind, ]
test <- mydata[-train_ind, ]

```

#### Fitting the model and plotting the curve
``` R
# show the active window
windows(title="Elodiff vs. outcome")

#plot the graph
plot(train$elo.diff, train$outcome, xlab="Elo difference", ylab="Probability of winning", xlim=c(-1000,1000),cex.lab=1.5)

# fit model 
model = glm(outcome ~ elo.diff, data = train, family = binomial, na.action = na.omit)

# Parameter estimates, statistical significance and other information
summary(model)

# plot prediction curve
curve(predict(model,data.frame(elo.diff=x),type="resp"),add=TRUE, col="black") 
points(train$elo.diff,fitted(model),pch=20, col='black')
```

#### Testing performance
```R
# Decision Boundary
fitted.results <- predict(model,newdata=subset(test,type='response'))
fitted.results <- ifelse(fitted.results > 0.5,1,0)
misClasificError <- mean(fitted.results != test$outcome)
print(paste('Accuracy',1-misClasificError))

# ROC
library(ROCR)
p <- predict(model, newdata=subset(test, type="response"))
pr <- prediction(p, mydata$outcome)
prf <- performance(pr, measure = "tpr", x.measure = "fpr")
windows(title="ROC curve")
plot(prf, cex.lab=1.5)

auc <- performance(pr, measure = "auc")
auc <- auc@y.values[[1]]
auc
```

#### Model Evaluation and Diagnostics
``` R
#Pseudo R^2
library(rcompanion)
nagelkerke(model)

# Wald test
# Note that testing p-values for a logistic regression uses Chi-square tests.  
# This is achieved through the test=“Wald” option in Anova to test the significance of 
# each coefficient (in our case, there is ony one), and the test=“Chisq” option in anova 
# for the significance of the overall model. 
library(car)
Anova(model, type="II", test="Wald")
```

#### Additional plotting functions

``` R
#plot logistic regression with library that will also show a histogram.
library(popbio)
logi.hist.plot(mydata$elo.diff, mydata$outcome, logi.mod = 1, type = "hist", boxp=FALSE, col="gray", xlab="ELO Difference")

----------------------------------------
# manually apply the curve from the model to remove dots from inital plot
windows(title="No Dots")
x <- c(-1000:1000)
b = model$coefficients[1]  # intercept
m = model$coefficients[2] # slope

y <- exp((b + m*x)) / (1 + exp((b + m*x)))

plot(x, y, xlab="Probability of winning", ylab="Elo Difference", pch = , ylim=c(0,1), xlim=c(-1000,1000), col='black', cex.lab=1.5)

```
